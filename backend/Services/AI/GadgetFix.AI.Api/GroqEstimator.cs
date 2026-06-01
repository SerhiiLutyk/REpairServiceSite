using System.Text;
using System.Text.Json;

namespace GadgetFix.AI.Api;

public interface IEstimateService
{
    Task<EstimateResult> EstimateAsync(EstimateRequest request, CancellationToken ct = default);
}

/// <summary>
/// Координатор оцінки: якщо налаштовано LLM (Groq) — питає модель,
/// інакше (або при помилці) використовує евристику <see cref="PriceEstimator"/>.
/// </summary>
public class EstimateService(
    GeminiEstimator gemini,
    GroqEstimator groq,
    PriceEstimator fallback,
    ILogger<EstimateService> logger) : IEstimateService
{
    public async Task<EstimateResult> EstimateAsync(EstimateRequest request, CancellationToken ct = default)
    {
        if (gemini.Enabled)
        {
            try { return await gemini.EstimateAsync(request, ct); }
            catch (Exception ex) { logger.LogWarning(ex, "Gemini недоступний — пробую Groq/евристику"); }
        }
        if (groq.Enabled)
        {
            try { return await groq.EstimateAsync(request, ct); }
            catch (Exception ex) { logger.LogWarning(ex, "Groq недоступний — фолбек на евристику"); }
        }
        return fallback.Estimate(request);
    }
}

/// <summary>Спільний промпт і парсинг JSON-відповіді мовних моделей.</summary>
public static class LlmEstimate
{
    public const string SystemPrompt =
        "Ти — досвідчений майстер сервісного центру з ремонту гаджетів в Україні. " +
        "Спершу ПРОАНАЛІЗУЙ опис поломки й подумай, яка несправність насправді ймовірна. Не вгадуй навмання.\n" +
        "ВАЖЛИВІ ПРАВИЛА:\n" +
        "- Якщо з опису ЗРОЗУМІЛО, що саме зламано (напр. «розбитий екран», «не тримає батарея», «не заряджається») — " +
        "оціни РЕАЛІСТИЧНУ вартість саме цього ремонту за цінами українського ринку 2024–2025 " +
        "для трьох варіантів запчастин: китайські дешеві, китайські середні, оригінальні.\n" +
        "- Пропонуй заміну конкретної деталі (екран, акумулятор тощо) ЛИШЕ якщо опис прямо на це вказує. " +
        "Не вигадуй заміну екрана, якщо про екран нічого не сказано.\n" +
        "- Якщо опис НЕЧІТКИЙ або недостатній (напр. «не працює», «щось зламалось», «глючить», «дивно поводиться») — " +
        "НЕ вигадуй конкретний ремонт. Замість цього запропонуй ДІАГНОСТИКУ: " +
        "у summary поясни, що точну причину й ціну можна визначити лише після діагностики майстром; " +
        "у options поверни ОДИН елемент з tier «Діагностика», невеликою вартістю (100–300 грн, часто безкоштовна при подальшому ремонті) " +
        "і встанови низький confidence (0.2–0.35).\n" +
        "- Прикинь ринкову вартість пристрою; вартість ремонту не повинна її перевищувати (зазвичай 10–45%).\n" +
        "- Не завищуй ціни; будь реалістичним.\n" +
        "Відповідай ЛИШЕ JSON-об'єктом без тексту навколо: " +
        "{\"summary\":\"коротко українською\",\"confidence\":число від 0 до 1," +
        "\"options\":[{\"tier\":\"назва варіанту\",\"min\":число,\"max\":число,\"description\":\"короткий опис українською\"}]} " +
        "— для зрозумілої поломки масив містить 3 варіанти запчастин, для нечіткого опису — один варіант «Діагностика».";

    public static string UserPrompt(EstimateRequest r) =>
        $"Тип: {r.DeviceType}. Модель: {r.Model ?? "невідомо"}. Поломка: {r.Problem}.";

    public static EstimateResult Parse(string content)
    {
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        var options = new List<PartOption>();
        if (root.TryGetProperty("options", out var opts) && opts.ValueKind == JsonValueKind.Array)
        {
            foreach (var o in opts.EnumerateArray())
            {
                options.Add(new PartOption(
                    o.TryGetProperty("tier", out var t) ? t.GetString() ?? "" : "",
                    o.TryGetProperty("min", out var mn) ? mn.GetDecimal() : 0m,
                    o.TryGetProperty("max", out var mx) ? mx.GetDecimal() : 0m,
                    o.TryGetProperty("description", out var d) ? d.GetString() ?? "" : ""));
            }
        }

        var summary = root.TryGetProperty("summary", out var s) ? s.GetString() ?? "" : "";
        var confidence = root.TryGetProperty("confidence", out var c) ? c.GetDouble() : 0.85;
        var min = options.Count > 0 ? options.Min(o => o.Min) : 0m;
        var max = options.Count > 0 ? options.Max(o => o.Max) : 0m;

        return new EstimateResult(Math.Round(min), Math.Round(max), "грн", summary, confidence, options);
    }
}

/// <summary>Виклик мовної моделі Groq (OpenAI-сумісний API) для оцінки вартості ремонту.</summary>
public class GroqEstimator(HttpClient http, IConfiguration config, ILogger<GroqEstimator> logger)
{
    private readonly string? _apiKey = config["Groq:ApiKey"];
    private readonly string _model = config["Groq:Model"] ?? "llama-3.3-70b-versatile";

    public bool Enabled => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<EstimateResult> EstimateAsync(EstimateRequest request, CancellationToken ct)
    {
        var payload = new
        {
            model = _model,
            temperature = 0.3,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = LlmEstimate.SystemPrompt },
                new { role = "user", content = LlmEstimate.UserPrompt(request) },
            },
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {_apiKey}");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync(ct);

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
            ?? throw new InvalidOperationException("Порожня відповідь моделі");

        logger.LogInformation("Groq оцінка отримана");
        return LlmEstimate.Parse(content);
    }
}

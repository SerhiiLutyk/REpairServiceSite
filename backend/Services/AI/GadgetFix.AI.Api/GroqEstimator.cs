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
        "ОБЕРИ ОДИН ІЗ ТРЬОХ СЦЕНАРІЇВ:\n" +
        "А) ЗАМІНА ЗАПЧАСТИНИ (екран, скло, акумулятор, камера, роз'єм тощо) — поверни ТРИ варіанти за якістю запчастини " +
        "з назвами рівно: \"Китайська якість\", \"Середня якість\", \"Оригінальні запчастини\".\n" +
        "Б) ПОСЛУГА БЕЗ ЗАМІНИ ДЕТАЛІ (чистка, видалення вологи, переустановка/налаштування ПЗ, оновлення, профілактика) — " +
        "поверни ОДИН варіант з tier \"Послуга\" і ФІКСОВАНОЮ (вузькою) ціною. НЕ ділити на якість запчастин.\n" +
        "В) НЕЧІТКИЙ опис («не працює», «щось зламалось», «глючить») — поверни ОДИН варіант tier \"Діагностика\" " +
        "(100–300 грн, часто безкоштовна при подальшому ремонті), confidence 0.2–0.35; у summary поясни потребу діагностики.\n" +
        "Пропонуй заміну деталі ЛИШЕ якщо опис прямо на це вказує.\n" +
        "ЦІНИ — реалістичні для українського ринку 2024–2026, ПОМІРНІ, не завищені. Орієнтири заміни екрана: " +
        "бюджетні смартфони 800–2500; середній клас 1500–4000; флагмани й iPhone — оригінал зазвичай 3500–7000 грн. " +
        "Заміна акумулятора 600–2000. Чистка/профілактика 300–800. Вартість ремонту НЕ повинна перевищувати вартість пристрою.\n" +
        "Відповідай ЛИШЕ JSON-об'єктом без тексту навколо: " +
        "{\"summary\":\"коротко українською\",\"confidence\":число від 0 до 1," +
        "\"options\":[{\"tier\":\"назва\",\"min\":число,\"max\":число,\"description\":\"короткий опис українською\"}]} " +
        "— сценарій А: 3 варіанти; сценарії Б і В: один варіант.";

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

    public async Task<string> ChatAsync(IEnumerable<ChatMessage> messages, CancellationToken ct)
    {
        const string system =
            "Ти — ввічливий онлайн-консультант сервісного центру GadgetFix (ремонт гаджетів в Україні). " +
            "Відповідай коротко українською про ремонт, орієнтовні ціни, терміни, гарантію та запис.";

        var msgs = new List<object> { new { role = "system", content = system } };
        msgs.AddRange(messages.Select(m => new { role = m.Role == "assistant" ? "assistant" : "user", content = m.Content }));

        var payload = new { model = _model, temperature = 0.6, messages = msgs };
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {_apiKey}");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "…";
    }

    public async Task<PhotoResult> AnalyzePhotoAsync(string base64, string mime, CancellationToken ct)
    {
        const string text =
            "Ти — експерт з ідентифікації електроніки. Уважно розглянь фото та визнач пристрій за логотипом, " +
            "кількістю й розташуванням камер, формою корпусу, кнопками та портами.\n" +
            "Зараз 2026 рік — існують новітні моделі, можливо новіші за твої знання. НІКОЛИ не кажи, що модель не існує; " +
            "якщо це нова версія — так і вкажи (напр. \"iPhone новітнього покоління, ймовірно Pro Max\").\n" +
            "ПРАВИЛА: deviceType (Смартфон/Ноутбук/Планшет/Смарт-годинник) визначай завжди; " +
            "визнач бренд за логотипом і вкажи якомога точнішу модель (бренд+серію, якщо не впевнений у поколінні); " +
            "ОЦІНИ СТАН: шукай видимі пошкодження (тріщини/подряпини екрана, розбитий дисплей, тріснута камера, " +
            "спухла батарея, деформований корпус, сліди вологи) і опиши в полі damage (або \"видимих пошкоджень не виявлено\"); " +
            "у note коротко українською поясни ознаки й рівень впевненості.\n" +
            "Відповідай ЛИШЕ JSON: {\"deviceType\":\"...\",\"model\":\"...\",\"note\":\"...\",\"damage\":\"...\"}.";

        var payload = new
        {
            model = "meta-llama/llama-4-scout-17b-16e-instruct",
            temperature = 0.2,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text },
                        new { type = "image_url", image_url = new { url = $"data:{mime};base64,{base64}" } },
                    },
                },
            },
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {_apiKey}");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";
        using var inner = JsonDocument.Parse(content);
        var r = inner.RootElement;
        return new PhotoResult(
            r.TryGetProperty("deviceType", out var d) ? d.GetString() : null,
            r.TryGetProperty("model", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() : null,
            r.TryGetProperty("note", out var n) ? n.GetString() ?? "" : "",
            r.TryGetProperty("damage", out var dm) && dm.ValueKind == JsonValueKind.String ? dm.GetString() : null);
    }

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

using System.Text;
using System.Text.Json;

namespace GadgetFix.AI.Api;

/// <summary>Виклик Google Gemini для оцінки вартості ремонту.</summary>
public class GeminiEstimator(HttpClient http, IConfiguration config, ILogger<GeminiEstimator> logger)
{
    private readonly string? _apiKey = config["Gemini:ApiKey"];
    private readonly string _model = config["Gemini:Model"] ?? "gemini-2.5-flash";

    public bool Enabled => !string.IsNullOrWhiteSpace(_apiKey);

    /// <summary>Чат підтримки на базі Gemini.</summary>
    public async Task<string> ChatAsync(IEnumerable<ChatMessage> messages, CancellationToken ct)
    {
        const string system =
            "Ти — ввічливий онлайн-консультант сервісного центру GadgetFix (ремонт гаджетів в Україні). " +
            "Відповідай коротко українською про ремонт, орієнтовні ціни, терміни, гарантію та запис. " +
            "Якщо питання не по темі — м'яко поверни до теми ремонту.";

        // Gemini вимагає, щоб історія починалася з повідомлення користувача
        var contents = messages
            .SkipWhile(m => m.Role != "user")
            .Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } },
            }).ToArray();

        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = system } } },
            contents,
            generationConfig = new { temperature = 0.6 },
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("x-goog-api-key", _apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content")
            .GetProperty("parts")[0].GetProperty("text").GetString() ?? "…";
    }

    /// <summary>Розпізнавання типу/моделі гаджета за фото (Gemini Vision).</summary>
    public async Task<PhotoResult> AnalyzePhotoAsync(string base64, string mime, CancellationToken ct)
    {
        const string prompt =
            "Ти — експерт з ідентифікації електроніки. Уважно й детально розглянь фото пристрою та визнач його.\n" +
            "На що дивитися: логотип бренду; кількість, розмір і розташування камер (вертикальний блок, квадратний модуль, коло тощо); " +
            "форма корпусу й рамок; виріз/отвір під фронтальну камеру; розташування кнопок і портів; написи на корпусі.\n" +
            "ПРАВИЛА:\n" +
            "1. deviceType визначай завжди (Смартфон / Ноутбук / Планшет / Смарт-годинник).\n" +
            "2. model вказуй ЛИШЕ якщо ти дійсно впевнений у конкретній моделі. " +
            "Якщо впевнений лише в бренді або серії — вкажи бренд/серію (напр. \"Samsung Galaxy серії S\"), а не конкретну модель. " +
            "Якщо визначити неможливо — постав model = null. НЕ ВИГАДУЙ модель навмання.\n" +
            "3. У полі note коротко українською поясни, за якими ознаками ти зробив висновок і наскільки впевнений.\n" +
            "Відповідай ЛИШЕ JSON: {\"deviceType\":\"...\",\"model\":\"... або null\",\"note\":\"...\"}.";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new { inline_data = new { mime_type = mime, data = base64 } },
                    },
                },
            },
            generationConfig = new { responseMimeType = "application/json", temperature = 0.1 },
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("x-goog-api-key", _apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var content = doc.RootElement.GetProperty("candidates")[0].GetProperty("content")
            .GetProperty("parts")[0].GetProperty("text").GetString() ?? "{}";

        using var inner = JsonDocument.Parse(content);
        var r = inner.RootElement;
        return new PhotoResult(
            r.TryGetProperty("deviceType", out var d) ? d.GetString() : null,
            r.TryGetProperty("model", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() : null,
            r.TryGetProperty("note", out var n) ? n.GetString() ?? "" : "");
    }

    public async Task<EstimateResult> EstimateAsync(EstimateRequest request, CancellationToken ct)
    {
        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = LlmEstimate.SystemPrompt } } },
            contents = new[] { new { parts = new[] { new { text = LlmEstimate.UserPrompt(request) } } } },
            generationConfig = new { responseMimeType = "application/json", temperature = 0.3 },
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("x-goog-api-key", _apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync(ct);

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("candidates")[0].GetProperty("content")
            .GetProperty("parts")[0].GetProperty("text").GetString()
            ?? throw new InvalidOperationException("Порожня відповідь моделі");

        logger.LogInformation("Gemini оцінка отримана");
        return LlmEstimate.Parse(content);
    }
}

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
            "Зараз 2026 рік. Існують НОВІ моделі (напр. iPhone 16/17, Samsung Galaxy S24/S25 тощо), які можуть бути " +
            "новішими за твої знання. НІКОЛИ не стверджуй, що модель «не існує» — якщо пристрій схожий на нову версію, " +
            "так і вкажи (напр. \"iPhone новітнього покоління, ймовірно Pro Max\"). Довіряй написам і логотипам на корпусі.\n" +
            "На що дивитися: логотип бренду; кількість, розмір і розташування камер (вертикальний блок, квадратний модуль, коло); " +
            "форма корпусу й рамок; виріз/отвір під фронтальну камеру; кнопки, порти; будь-які написи/маркування.\n" +
            "ПРАВИЛА:\n" +
            "1. deviceType визначай завжди (Смартфон / Ноутбук / Планшет / Смарт-годинник).\n" +
            "2. Визнач бренд за логотипом. model вказуй якомога точніше; якщо не впевнений у точному поколінні — " +
            "вкажи бренд + серію/орієнтовну модель (напр. \"Apple iPhone (Pro Max, новітнє покоління)\"). " +
            "Не заперечуй існування і не лишай null, якщо видно хоча б бренд.\n" +
            "3. ОЦІНИ ВІЗУАЛЬНИЙ СТАН: уважно шукай видимі пошкодження — тріщини чи подряпини на екрані/склі, " +
            "розбитий дисплей, пошкоджена/тріснута камера, здута (спухла) батарея, деформований/зігнутий корпус, " +
            "сліди вологи чи корозії, відсутні елементи. Опиши знайдене стисло українською у полі damage. " +
            "Якщо видимих пошкоджень немає — damage = \"видимих пошкоджень не виявлено\".\n" +
            "4. У note коротко українською поясни ознаки ідентифікації й рівень впевненості.\n" +
            "Відповідай ЛИШЕ JSON: {\"deviceType\":\"...\",\"model\":\"...\",\"note\":\"...\",\"damage\":\"...\"}.";

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
            r.TryGetProperty("note", out var n) ? n.GetString() ?? "" : "",
            r.TryGetProperty("damage", out var dm) && dm.ValueKind == JsonValueKind.String ? dm.GetString() : null);
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

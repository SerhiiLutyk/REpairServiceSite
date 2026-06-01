using System.Text;
using System.Text.Json;

namespace GadgetFix.AI.Api;

/// <summary>Виклик Google Gemini для оцінки вартості ремонту.</summary>
public class GeminiEstimator(HttpClient http, IConfiguration config, ILogger<GeminiEstimator> logger)
{
    private readonly string? _apiKey = config["Gemini:ApiKey"];
    private readonly string _model = config["Gemini:Model"] ?? "gemini-2.0-flash";

    public bool Enabled => !string.IsNullOrWhiteSpace(_apiKey);

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

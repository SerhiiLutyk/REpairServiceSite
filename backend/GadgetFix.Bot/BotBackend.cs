using System.Text;
using System.Text.Json;

namespace GadgetFix.Bot;

public record BotUser(string Id, string FullName, string Phone, string? Email);
public record BotOrder(string Problem, int Status, decimal? Price, DateTime CreatedAt);
public record BotEstimate(decimal Min, decimal Max, string Summary, IReadOnlyList<(string Tier, decimal Min, decimal Max)> Options);

/// <summary>Звернення бота до мікросервісів (Users, Orders, AI) через внутрішні ендпоінти.</summary>
public class BotBackend(IHttpClientFactory factory)
{
    public static readonly string[] StatusLabels =
        ["Нова заявка", "Діагностика", "В ремонті", "Готово", "Видано", "Скасовано"];

    public async Task<BotUser?> GetUserByChatAsync(long chatId, CancellationToken ct)
    {
        var resp = await factory.CreateClient("users").GetAsync($"/internal/users/by-telegram/{chatId}", ct);
        if (!resp.IsSuccessStatusCode) return null;
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var r = doc.RootElement;
        return new BotUser(
            r.GetProperty("id").GetString()!,
            r.GetProperty("fullName").GetString() ?? "",
            r.GetProperty("phone").GetString() ?? "",
            r.TryGetProperty("email", out var e) ? e.GetString() : null);
    }

    public async Task<BotUser?> LinkAsync(string code, long chatId, CancellationToken ct)
    {
        var payload = new { code, chatId = chatId.ToString() };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var resp = await factory.CreateClient("users").PostAsync("/internal/users/telegram-link", content, ct);
        if (!resp.IsSuccessStatusCode) return null;
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var r = doc.RootElement;
        return new BotUser(r.GetProperty("id").GetString()!, r.GetProperty("fullName").GetString() ?? "",
            r.GetProperty("phone").GetString() ?? "", null);
    }

    public async Task<IReadOnlyList<BotOrder>> GetOrdersAsync(string userId, CancellationToken ct)
    {
        var resp = await factory.CreateClient("orders").GetAsync($"/internal/orders/by-user/{userId}", ct);
        if (!resp.IsSuccessStatusCode) return [];
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return doc.RootElement.EnumerateArray().Select(o => new BotOrder(
            o.GetProperty("problemDescription").GetString() ?? "",
            o.GetProperty("status").GetInt32(),
            o.TryGetProperty("estimatedPrice", out var p) && p.ValueKind != JsonValueKind.Null ? p.GetDecimal() : null,
            o.GetProperty("createdAt").GetDateTime())).ToList();
    }

    public async Task<BotEstimate?> EstimateAsync(string device, string? model, string problem, CancellationToken ct)
    {
        var payload = new { deviceType = device, model, problem };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var resp = await factory.CreateClient("ai").PostAsync("/api/ai/estimate", content, ct);
        if (!resp.IsSuccessStatusCode) return null;
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var r = doc.RootElement;
        var opts = new List<(string, decimal, decimal)>();
        if (r.TryGetProperty("options", out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var o in arr.EnumerateArray())
                opts.Add((o.GetProperty("tier").GetString() ?? "", o.GetProperty("min").GetDecimal(), o.GetProperty("max").GetDecimal()));
        return new BotEstimate(r.GetProperty("min").GetDecimal(), r.GetProperty("max").GetDecimal(),
            r.TryGetProperty("explanation", out var ex) ? ex.GetString() ?? "" : "", opts);
    }
}

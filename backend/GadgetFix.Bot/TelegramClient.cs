using System.Text;
using System.Text.Json;

namespace GadgetFix.Bot;

public class BotOptions
{
    public string Token { get; set; } = "";
    public bool Enabled => !string.IsNullOrWhiteSpace(Token);
}

/// <summary>Тонка обгортка над Telegram Bot API (long polling + надсилання).</summary>
public class TelegramClient(IHttpClientFactory factory, BotOptions options)
{
    private string Base => $"https://api.telegram.org/bot{options.Token}";

    public async Task<JsonElement[]> GetUpdatesAsync(long offset, CancellationToken ct)
    {
        var http = factory.CreateClient("telegram");
        http.Timeout = TimeSpan.FromSeconds(40);
        var resp = await http.GetAsync($"{Base}/getUpdates?offset={offset}&timeout=30", ct);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty("result").EnumerateArray().Select(e => e.Clone()).ToArray();
    }

    public async Task SendAsync(long chatId, string text, string[][]? keyboard = null, CancellationToken ct = default)
    {
        var http = factory.CreateClient("telegram");
        object replyMarkup = keyboard is null
            ? new { remove_keyboard = false }
            : new { keyboard, resize_keyboard = true };

        var payload = new
        {
            chat_id = chatId,
            text,
            parse_mode = "HTML",
            reply_markup = replyMarkup,
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        await http.PostAsync($"{Base}/sendMessage", content, ct);
    }
}

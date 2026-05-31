namespace GadgetFix.Notifications.Api;

public class TelegramOptions
{
    public string? BotToken { get; set; }
    public string? ChatId { get; set; }
}

public interface ITelegramSender
{
    Task<bool> SendAsync(string text, CancellationToken ct = default);
}

public class TelegramSender(HttpClient http, TelegramOptions options, ILogger<TelegramSender> logger) : ITelegramSender
{
    public async Task<bool> SendAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.BotToken) || string.IsNullOrWhiteSpace(options.ChatId))
        {
            // Токен не налаштовано (напр. локальна розробка) — лише логуємо
            logger.LogInformation("Telegram не налаштовано. Повідомлення: {Text}", text);
            return false;
        }

        var url = $"https://api.telegram.org/bot{options.BotToken}/sendMessage";
        var payload = new { chat_id = options.ChatId, text };
        var response = await http.PostAsJsonAsync(url, payload, ct);

        if (!response.IsSuccessStatusCode)
            logger.LogWarning("Telegram API повернув {Status}", response.StatusCode);

        return response.IsSuccessStatusCode;
    }
}

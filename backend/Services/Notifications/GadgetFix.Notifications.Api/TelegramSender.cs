namespace GadgetFix.Notifications.Api;

public class TelegramOptions
{
    public string? BotToken { get; set; }
    public string? ChatId { get; set; }
}

public interface ITelegramSender
{
    Task<bool> SendAsync(string text, string? chatId = null, CancellationToken ct = default);
}

public class TelegramSender(HttpClient http, TelegramOptions options, ILogger<TelegramSender> logger) : ITelegramSender
{
    public async Task<bool> SendAsync(string text, string? chatId = null, CancellationToken ct = default)
    {
        // Персональний chat_id клієнта має пріоритет над дефолтним з конфігу
        var target = string.IsNullOrWhiteSpace(chatId) ? options.ChatId : chatId;

        if (string.IsNullOrWhiteSpace(options.BotToken) || string.IsNullOrWhiteSpace(target))
        {
            // Токен/чат не налаштовано (напр. локальна розробка) — лише логуємо
            logger.LogInformation("Telegram не налаштовано. Повідомлення для {Chat}: {Text}", target ?? "—", text);
            return false;
        }

        var url = $"https://api.telegram.org/bot{options.BotToken}/sendMessage";
        var payload = new { chat_id = target, text };
        var response = await http.PostAsJsonAsync(url, payload, ct);

        if (!response.IsSuccessStatusCode)
            logger.LogWarning("Telegram API повернув {Status}", response.StatusCode);

        return response.IsSuccessStatusCode;
    }
}

using System.Text.Json;
using GadgetFix.Orders.BLL;

namespace GadgetFix.Orders.Api;

/// <summary>
/// При готовності замовлення дізнається Telegram chat_id клієнта в Users-сервісі
/// та надсилає персональний пуш через сервіс нотифікацій.
/// </summary>
public class HttpOrderNotifier(IHttpClientFactory factory, ILogger<HttpOrderNotifier> logger) : IOrderNotifier
{
    public async Task NotifyReadyAsync(OrderDto order, CancellationToken ct = default)
    {
        try
        {
            string? chatId = await ResolveChatIdAsync(order.UserId, ct);

            var text =
                $"✅ Ваше замовлення готове!\n" +
                $"Клієнт: {order.CustomerName}\n" +
                $"Телефон: {order.Phone}\n" +
                $"№ {order.Id}";

            var notifications = factory.CreateClient("notifications");
            await notifications.PostAsJsonAsync("/api/notifications/telegram", new { text, chatId }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не вдалося надіслати нотифікацію для замовлення {OrderId}", order.Id);
        }
    }

    private async Task<string?> ResolveChatIdAsync(Guid? userId, CancellationToken ct)
    {
        if (userId is null) return null;
        try
        {
            var users = factory.CreateClient("users");
            var resp = await users.GetAsync($"/api/users/{userId}", ct);
            if (!resp.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            return doc.RootElement.TryGetProperty("telegramChatId", out var c) ? c.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}

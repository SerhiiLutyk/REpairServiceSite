using GadgetFix.Orders.BLL;

namespace GadgetFix.Orders.Api;

/// <summary>Надсилає запит у сервіс нотифікацій (Telegram) через HTTP + service discovery.</summary>
public class HttpOrderNotifier(HttpClient http, ILogger<HttpOrderNotifier> logger) : IOrderNotifier
{
    public async Task NotifyReadyAsync(OrderDto order, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                text = $"✅ Замовлення готове!\nКлієнт: {order.CustomerName}\nТелефон: {order.Phone}\n№ {order.Id}",
            };
            await http.PostAsJsonAsync("/api/notifications/telegram", payload, ct);
        }
        catch (Exception ex)
        {
            // Нотифікація не критична — лише логуємо
            logger.LogWarning(ex, "Не вдалося надіслати нотифікацію для замовлення {OrderId}", order.Id);
        }
    }
}

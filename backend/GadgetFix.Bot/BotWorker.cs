using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace GadgetFix.Bot;

public class BotWorker(TelegramClient tg, BotBackend backend, BotOptions options, ILogger<BotWorker> logger)
    : BackgroundService
{
    private long _offset;

    private enum Step { None, Device, Model, Problem }
    private sealed class Conv { public Step Step; public string Device = ""; public string? Model; }
    private readonly ConcurrentDictionary<long, Conv> _state = new();

    private static readonly string[][] MainMenu =
    [
        ["👤 Акаунт", "📦 Мої замовлення"],
        ["💰 Оцінка ремонту"],
        ["🔗 Прив'язати акаунт", "ℹ️ Про сервіс"],
    ];
    private static readonly string[][] DeviceMenu =
    [
        ["Смартфон", "Ноутбук"],
        ["Планшет", "Смарт-годинник"],
    ];

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!options.Enabled)
        {
            logger.LogWarning("Telegram BotToken не налаштовано — бот вимкнено.");
            return;
        }
        logger.LogInformation("Telegram-бот запущено (long polling).");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var updates = await tg.GetUpdatesAsync(_offset, ct);
                foreach (var u in updates)
                {
                    _offset = u.GetProperty("update_id").GetInt64() + 1;
                    if (u.TryGetProperty("message", out var msg) && msg.TryGetProperty("text", out var t))
                        await HandleAsync(msg.GetProperty("chat").GetProperty("id").GetInt64(), t.GetString() ?? "", ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Помилка циклу бота");
                await Task.Delay(2000, ct);
            }
        }
    }

    private async Task HandleAsync(long chat, string text, CancellationToken ct)
    {
        text = text.Trim();

        // /start [код] — звичайний старт або привʼязка через deep-link (t.me/bot?start=КОД)
        if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                await LinkAsync(chat, parts[1].Trim(), ct);
                return;
            }
            _state.TryRemove(chat, out _);
            await tg.SendAsync(chat,
                "👋 Вітаємо у <b>GadgetFix</b> — сервіс ремонту гаджетів.\nОберіть дію:", MainMenu, ct);
            return;
        }

        // Пункти меню мають пріоритет над станом діалогу
        switch (text)
        {
            case "👤 Акаунт":
                _state.TryRemove(chat, out _); await ShowAccountAsync(chat, ct); return;
            case "📦 Мої замовлення":
                _state.TryRemove(chat, out _); await ShowOrdersAsync(chat, ct); return;
            case "💰 Оцінка ремонту":
                _state[chat] = new Conv { Step = Step.Device };
                await tg.SendAsync(chat, "Оберіть тип гаджета:", DeviceMenu, ct); return;
            case "🔗 Прив'язати акаунт":
                _state.TryRemove(chat, out _);
                await tg.SendAsync(chat,
                    "Щоб прив'язати акаунт:\n1. Увійдіть на сайті → <b>Кабінет</b>\n2. Натисніть «Прив'язати Telegram» і скопіюйте код\n3. Надішліть сюди: <code>/link КОД</code>", MainMenu, ct);
                return;
            case "ℹ️ Про сервіс":
                _state.TryRemove(chat, out _);
                await tg.SendAsync(chat,
                    "🔧 <b>GadgetFix</b> — ремонт смартфонів, ноутбуків, планшетів.\n• AI-оцінка вартості\n• Онлайн-запис\n• Гарантія до 6 міс.\n📞 +380 00 000 0000", MainMenu, ct);
                return;
        }

        if (text.StartsWith("/link ", StringComparison.OrdinalIgnoreCase))
        {
            await LinkAsync(chat, text[6..].Trim(), ct);
            return;
        }

        // Діалог оцінки вартості
        if (_state.TryGetValue(chat, out var conv))
        {
            await ContinueEstimateAsync(chat, conv, text, ct);
            return;
        }

        await tg.SendAsync(chat, "Скористайтесь меню нижче 👇", MainMenu, ct);
    }

    private async Task LinkAsync(long chat, string code, CancellationToken ct)
    {
        var user = await backend.LinkAsync(code, chat, ct);
        await tg.SendAsync(chat, user is null
            ? "❌ Невірний або застарілий код. Згенеруйте новий у кабінеті."
            : $"✅ Акаунт прив'язано: <b>{user.FullName}</b>. Тепер ви отримуватимете сповіщення про готовність замовлень.",
            MainMenu, ct);
    }

    private async Task ShowAccountAsync(long chat, CancellationToken ct)
    {
        var user = await backend.GetUserByChatAsync(chat, ct);
        if (user is null)
        {
            await tg.SendAsync(chat, "Акаунт не прив'язано. Натисніть «🔗 Прив'язати акаунт».", MainMenu, ct);
            return;
        }
        var sb = new StringBuilder();
        sb.AppendLine("👤 <b>Ваш акаунт</b>");
        sb.AppendLine($"Ім'я: {user.FullName}");
        sb.AppendLine($"Телефон: {user.Phone}");
        if (!string.IsNullOrWhiteSpace(user.Email)) sb.AppendLine($"Email: {user.Email}");
        await tg.SendAsync(chat, sb.ToString(), MainMenu, ct);
    }

    private async Task ShowOrdersAsync(long chat, CancellationToken ct)
    {
        var user = await backend.GetUserByChatAsync(chat, ct);
        if (user is null)
        {
            await tg.SendAsync(chat, "Акаунт не прив'язано. Натисніть «🔗 Прив'язати акаунт».", MainMenu, ct);
            return;
        }
        var orders = await backend.GetOrdersAsync(user.Id, ct);
        if (orders.Count == 0)
        {
            await tg.SendAsync(chat, "У вас поки немає замовлень.", MainMenu, ct);
            return;
        }
        var sb = new StringBuilder("📦 <b>Ваші замовлення</b>\n");
        foreach (var o in orders)
        {
            var label = o.Status >= 0 && o.Status < BotBackend.StatusLabels.Length ? BotBackend.StatusLabels[o.Status] : "—";
            sb.AppendLine($"\n• {o.Problem}\n  Статус: <b>{label}</b> · {o.CreatedAt:dd.MM.yyyy}{(o.Price is not null ? $" · ~{o.Price} грн" : "")}");
        }
        await tg.SendAsync(chat, sb.ToString(), MainMenu, ct);
    }

    private async Task ContinueEstimateAsync(long chat, Conv conv, string text, CancellationToken ct)
    {
        switch (conv.Step)
        {
            case Step.Device:
                conv.Device = text;
                conv.Step = Step.Model;
                await tg.SendAsync(chat, "Вкажіть модель (або надішліть «-» щоб пропустити):", null, ct);
                break;
            case Step.Model:
                conv.Model = text == "-" ? null : text;
                conv.Step = Step.Problem;
                await tg.SendAsync(chat, "Опишіть, що сталося з гаджетом:", null, ct);
                break;
            case Step.Problem:
                await tg.SendAsync(chat, "⏳ Рахую вартість…", null, ct);
                var est = await backend.EstimateAsync(conv.Device, conv.Model, text, ct);
                _state.TryRemove(chat, out _);
                if (est is null)
                {
                    await tg.SendAsync(chat, "Не вдалося оцінити. Спробуйте пізніше.", MainMenu, ct);
                    return;
                }
                var sb = new StringBuilder();
                sb.AppendLine($"💰 <b>Орієнтовно: {est.Min}–{est.Max} грн</b>");
                if (!string.IsNullOrWhiteSpace(est.Summary)) sb.AppendLine(est.Summary);
                foreach (var (tier, min, max) in est.Options)
                    sb.AppendLine($"\n• <b>{tier}</b>: {min}–{max} грн");
                await tg.SendAsync(chat, sb.ToString(), MainMenu, ct);
                break;
        }
    }
}

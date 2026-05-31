using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.Notifications.Api.Controllers;

public record TelegramMessage(string Text);

[ApiController]
[Route("api/notifications")]
public class NotificationsController(ITelegramSender telegram) : ControllerBase
{
    [HttpPost("telegram")]
    public async Task<ActionResult> SendTelegram(TelegramMessage message, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
            return BadRequest(new { error = "Порожнє повідомлення." });

        var sent = await telegram.SendAsync(message.Text, ct);
        return Ok(new { sent });
    }
}

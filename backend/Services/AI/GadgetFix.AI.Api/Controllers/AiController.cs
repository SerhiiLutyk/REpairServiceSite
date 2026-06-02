using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.AI.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController(IEstimateService estimator, GeminiEstimator gemini) : ControllerBase
{
    /// <summary>Оцінка приблизної вартості ремонту за описом (LLM Groq з фолбеком на евристику).</summary>
    [HttpPost("estimate")]
    public async Task<ActionResult<EstimateResult>> Estimate(EstimateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceType) || string.IsNullOrWhiteSpace(request.Problem))
            return BadRequest(new { error = "Вкажіть тип гаджета та опис поломки." });

        return Ok(await estimator.EstimateAsync(request, ct));
    }

    /// <summary>Чат підтримки.</summary>
    [HttpPost("chat")]
    public async Task<ActionResult<ChatReply>> Chat(ChatRequest request, CancellationToken ct)
    {
        if (request.Messages is null || request.Messages.Count == 0)
            return BadRequest(new { error = "Порожнє повідомлення." });
        if (!gemini.Enabled)
            return Ok(new ChatReply("Вітаю! Зараз консультант недоступний. Опишіть проблему через AI-калькулятор або залиште заявку."));

        try
        {
            var take = request.Messages.TakeLast(12).ToList();
            return Ok(new ChatReply(await gemini.ChatAsync(take, ct)));
        }
        catch
        {
            return Ok(new ChatReply("Вибачте, сталася помилка. Спробуйте ще раз."));
        }
    }

    /// <summary>Аналіз фото гаджета для визначення типу/моделі (Gemini Vision).</summary>
    [HttpPost("analyze-photo")]
    public async Task<ActionResult<PhotoResult>> AnalyzePhoto(PhotoRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
            return BadRequest(new { error = "Фото не передано." });
        if (!gemini.Enabled)
            return Ok(new PhotoResult(null, null, "Розпізнавання за фото недоступне (немає AI-ключа)."));

        try
        {
            var result = await gemini.AnalyzePhotoAsync(request.ImageBase64, request.MimeType ?? "image/jpeg", ct);
            return Ok(result);
        }
        catch
        {
            return Ok(new PhotoResult(null, null, "Не вдалося розпізнати гаджет. Вкажіть тип вручну."));
        }
    }
}

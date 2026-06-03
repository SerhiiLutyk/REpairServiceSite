using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.AI.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController(IEstimateService estimator, GeminiEstimator gemini, GroqEstimator groq) : ControllerBase
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

        var take = request.Messages.TakeLast(12).ToList();

        // Спершу Gemini, при помилці — Groq
        if (gemini.Enabled)
        {
            try { return Ok(new ChatReply(await gemini.ChatAsync(take, ct))); }
            catch { /* фолбек нижче */ }
        }
        if (groq.Enabled)
        {
            try { return Ok(new ChatReply(await groq.ChatAsync(take, ct))); }
            catch { /* фолбек нижче */ }
        }
        return Ok(new ChatReply("Вітаю! Зараз консультант недоступний. Скористайтесь AI-калькулятором або залиште заявку."));
    }

    /// <summary>Аналіз фото гаджета для визначення типу/моделі (Gemini Vision).</summary>
    [HttpPost("analyze-photo")]
    public async Task<ActionResult<PhotoResult>> AnalyzePhoto(PhotoRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
            return BadRequest(new { error = "Фото не передано." });

        var mime = request.MimeType ?? "image/jpeg";
        if (gemini.Enabled)
        {
            try { return Ok(await gemini.AnalyzePhotoAsync(request.ImageBase64, mime, ct)); }
            catch { /* фолбек на Groq */ }
        }
        if (groq.Enabled)
        {
            try { return Ok(await groq.AnalyzePhotoAsync(request.ImageBase64, mime, ct)); }
            catch { /* фолбек нижче */ }
        }
        return Ok(new PhotoResult(null, null, "Не вдалося розпізнати гаджет. Вкажіть тип вручну."));
    }
}

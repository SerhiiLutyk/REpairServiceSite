using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.AI.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController(IEstimateService estimator) : ControllerBase
{
    /// <summary>Оцінка приблизної вартості ремонту за описом (LLM Groq з фолбеком на евристику).</summary>
    [HttpPost("estimate")]
    public async Task<ActionResult<EstimateResult>> Estimate(EstimateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceType) || string.IsNullOrWhiteSpace(request.Problem))
            return BadRequest(new { error = "Вкажіть тип гаджета та опис поломки." });

        return Ok(await estimator.EstimateAsync(request, ct));
    }

    /// <summary>
    /// Аналіз фото задньої кришки для визначення типу гаджета.
    /// Поки що повертає заглушку; передбачено інтеграцію з vision-моделлю.
    /// </summary>
    [HttpPost("analyze-photo")]
    public ActionResult AnalyzePhoto()
    {
        return Ok(new
        {
            detected = (string?)null,
            message = "Розпізнавання за фото буде доступне найближчим часом. Поки вкажіть тип гаджета вручну.",
        });
    }
}

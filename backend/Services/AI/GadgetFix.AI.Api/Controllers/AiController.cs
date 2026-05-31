using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.AI.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController(PriceEstimator estimator) : ControllerBase
{
    /// <summary>Оцінка приблизної вартості ремонту за описом.</summary>
    [HttpPost("estimate")]
    public ActionResult<EstimateResult> Estimate(EstimateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceType) || string.IsNullOrWhiteSpace(request.Problem))
            return BadRequest(new { error = "Вкажіть тип гаджета та опис поломки." });

        return Ok(estimator.Estimate(request));
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

using System.Security.Claims;
using GadgetFix.Reviews.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.Reviews.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(IReviewService reviews) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ReviewDto>> GetRecent(CancellationToken ct) =>
        await reviews.GetRecentAsync(20, ct);

    [HttpGet("stats")]
    public async Task<ReviewStats> GetStats(CancellationToken ct) =>
        await reviews.GetStatsAsync(ct);

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewRequest req, CancellationToken ct)
    {
        var name = User.FindFirstValue(ClaimTypes.Name) ?? "Клієнт";
        Guid? userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var g) ? g : null;
        if (string.IsNullOrWhiteSpace(req.Comment))
            return BadRequest(new { error = "Напишіть відгук." });
        return Ok(await reviews.CreateAsync(userId, name, req, ct));
    }
}

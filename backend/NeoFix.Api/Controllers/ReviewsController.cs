using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Data;
using NeoFix.Api.Dto;
using NeoFix.Api.Extensions;
using NeoFix.Api.Models;

namespace NeoFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<object>>> List()
    {
        var rows = await db.Reviews.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(rows.Select(r => new
        {
            id = r.Id,
            rating = r.Rating,
            comment = r.Comment,
            created_at = r.CreatedAt,
            user_id = r.UserId,
        }));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create([FromBody] CreateReviewRequest request)
    {
        var review = new Review
        {
            UserId = User.GetUserId(),
            Rating = request.Rating,
            Comment = request.Comment,
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { id = review.Id }, new { id = review.Id });
    }
}

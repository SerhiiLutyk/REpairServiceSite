using GadgetFix.Reviews.DAL;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Reviews.BLL;

public record CreateReviewRequest(int Rating, string Comment);
public record ReviewDto(int Id, string AuthorName, int Rating, string Comment, DateTime CreatedAt);
public record ReviewStats(double Average, int Count);

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetRecentAsync(int take = 20, CancellationToken ct = default);
    Task<ReviewStats> GetStatsAsync(CancellationToken ct = default);
    Task<ReviewDto> CreateAsync(Guid? userId, string authorName, CreateReviewRequest req, CancellationToken ct = default);
}

public class ReviewService(ReviewsDbContext db) : IReviewService
{
    public async Task<IReadOnlyList<ReviewDto>> GetRecentAsync(int take = 20, CancellationToken ct = default) =>
        await db.Reviews.AsNoTracking().OrderByDescending(r => r.CreatedAt).Take(take)
            .Select(r => new ReviewDto(r.Id, r.AuthorName, r.Rating, r.Comment, r.CreatedAt)).ToListAsync(ct);

    public async Task<ReviewStats> GetStatsAsync(CancellationToken ct = default)
    {
        if (!await db.Reviews.AnyAsync(ct)) return new ReviewStats(0, 0);
        return new ReviewStats(
            Math.Round(await db.Reviews.AverageAsync(r => r.Rating, ct), 1),
            await db.Reviews.CountAsync(ct));
    }

    public async Task<ReviewDto> CreateAsync(Guid? userId, string authorName, CreateReviewRequest req, CancellationToken ct = default)
    {
        var rating = Math.Clamp(req.Rating, 1, 5);
        var review = new Review { UserId = userId, AuthorName = authorName, Rating = rating, Comment = req.Comment.Trim() };
        db.Reviews.Add(review);
        await db.SaveChangesAsync(ct);
        return new ReviewDto(review.Id, review.AuthorName, review.Rating, review.Comment, review.CreatedAt);
    }
}

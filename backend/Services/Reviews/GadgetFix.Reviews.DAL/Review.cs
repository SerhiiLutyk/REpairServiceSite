using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Reviews.DAL;

public class Review
{
    public int Id { get; set; }
    public Guid? UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }            // 1..5
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ReviewsDbContext(DbContextOptions<ReviewsDbContext> options) : DbContext(options)
{
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.AuthorName).HasMaxLength(120).IsRequired();
            e.Property(r => r.Comment).HasMaxLength(1000);
            e.HasIndex(r => r.CreatedAt);
        });
    }
}

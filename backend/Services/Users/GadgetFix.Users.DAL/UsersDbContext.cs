using GadgetFix.Users.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Users.DAL;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Phone).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(120).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150);
        });
    }
}

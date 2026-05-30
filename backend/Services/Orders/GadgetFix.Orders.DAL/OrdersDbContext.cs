using GadgetFix.Orders.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Orders.DAL;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.CustomerName).HasMaxLength(120).IsRequired();
            e.Property(o => o.Phone).HasMaxLength(20).IsRequired();
            e.Property(o => o.ProblemDescription).HasMaxLength(1000);
            e.Property(o => o.EstimatedPrice).HasColumnType("numeric(10,2)");
            e.HasIndex(o => o.Status);
        });
    }
}

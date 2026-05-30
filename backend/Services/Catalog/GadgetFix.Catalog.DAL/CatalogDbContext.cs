using GadgetFix.Catalog.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Catalog.DAL;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<RepairService> RepairServices => Set<RepairService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceType>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(80).IsRequired();
            e.Property(d => d.Slug).HasMaxLength(40).IsRequired();
            e.HasIndex(d => d.Slug).IsUnique();
        });

        modelBuilder.Entity<RepairService>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).HasMaxLength(120).IsRequired();
            e.Property(s => s.BasePrice).HasColumnType("numeric(10,2)");
            e.HasOne(s => s.DeviceType).WithMany(d => d.Services).HasForeignKey(s => s.DeviceTypeId);
        });

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder mb)
    {
        mb.Entity<DeviceType>().HasData(
            new DeviceType { Id = 1, Name = "Смартфон", Slug = "smartphone", Icon = "smartphone" },
            new DeviceType { Id = 2, Name = "Ноутбук", Slug = "laptop", Icon = "laptop" },
            new DeviceType { Id = 3, Name = "Планшет", Slug = "tablet", Icon = "tablet" },
            new DeviceType { Id = 4, Name = "Смарт-годинник", Slug = "watch", Icon = "watch" }
        );

        mb.Entity<RepairService>().HasData(
            new RepairService { Id = 1, DeviceTypeId = 1, Name = "Заміна екрана", BasePrice = 1800m, EstimatedDays = 1 },
            new RepairService { Id = 2, DeviceTypeId = 1, Name = "Заміна акумулятора", BasePrice = 900m, EstimatedDays = 1 },
            new RepairService { Id = 3, DeviceTypeId = 1, Name = "Ремонт після потрапляння води", BasePrice = 1500m, EstimatedDays = 3 },
            new RepairService { Id = 4, DeviceTypeId = 2, Name = "Чистка та заміна термопасти", BasePrice = 700m, EstimatedDays = 1 },
            new RepairService { Id = 5, DeviceTypeId = 2, Name = "Апгрейд SSD / RAM", BasePrice = 1200m, EstimatedDays = 1 },
            new RepairService { Id = 6, DeviceTypeId = 3, Name = "Заміна тачскріна", BasePrice = 1400m, EstimatedDays = 2 },
            new RepairService { Id = 7, DeviceTypeId = 4, Name = "Заміна скла дисплея", BasePrice = 1100m, EstimatedDays = 2 }
        );
    }
}

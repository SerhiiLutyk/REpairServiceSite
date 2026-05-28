using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Models;

namespace NeoFix.Api.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<UserRole> AppUserRoles => Set<UserRole>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Profile>(e =>
        {
            e.ToTable("profiles");
            e.HasKey(p => p.Id);
            e.Property(p => p.FullName).HasColumnName("full_name");
            e.Property(p => p.PhoneNumber).HasColumnName("phone_number");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.HasOne(p => p.User).WithOne(u => u.Profile).HasForeignKey<Profile>(p => p.Id);
        });

        builder.Entity<UserRole>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(r => r.Id);
            e.Property(r => r.UserId).HasColumnName("user_id");
            e.HasIndex(r => new { r.UserId, r.Role }).IsUnique();
            e.HasOne(r => r.User).WithMany(u => u.Roles).HasForeignKey(r => r.UserId);
        });

        builder.Entity<Service>(e =>
        {
            e.ToTable("services");
            e.HasKey(s => s.Id);
            e.Property(s => s.DeviceType).HasColumnName("device_type");
            e.Property(s => s.ServiceName).HasColumnName("service_name");
            e.Property(s => s.EstimatedPrice).HasColumnName("estimated_price").HasPrecision(10, 2);
            e.Property(s => s.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            e.Property(s => s.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<Booking>(e =>
        {
            e.ToTable("bookings");
            e.HasKey(b => b.Id);
            e.Property(b => b.UserId).HasColumnName("user_id");
            e.Property(b => b.ServiceId).HasColumnName("service_id");
            e.Property(b => b.BookingDate).HasColumnName("booking_date");
            e.Property(b => b.BookingTime).HasColumnName("booking_time");
            e.Property(b => b.MasterNotes).HasColumnName("master_notes");
            e.Property(b => b.CreatedAt).HasColumnName("created_at");
            e.HasIndex(b => new { b.BookingDate, b.BookingTime }).IsUnique();
            e.HasOne(b => b.User).WithMany(u => u.Bookings).HasForeignKey(b => b.UserId);
            e.HasOne(b => b.Service).WithMany(s => s.Bookings).HasForeignKey(b => b.ServiceId);
        });

        builder.Entity<Review>(e =>
        {
            e.ToTable("reviews");
            e.HasKey(r => r.Id);
            e.Property(r => r.UserId).HasColumnName("user_id");
            e.Property(r => r.CreatedAt).HasColumnName("created_at");
            e.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
        });
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Models;

namespace NeoFix.Api.Data;

public static class DbSeed
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        if (await db.Services.AnyAsync())
            return;

        db.Services.AddRange(
            new Service
            {
                DeviceType = "Smartphone",
                ServiceName = "Screen replacement",
                EstimatedPrice = 89,
                EstimatedDurationMinutes = 90,
            },
            new Service
            {
                DeviceType = "Smartphone",
                ServiceName = "Battery replacement",
                EstimatedPrice = 49,
                EstimatedDurationMinutes = 45,
            },
            new Service
            {
                DeviceType = "Laptop",
                ServiceName = "SSD upgrade",
                EstimatedPrice = 120,
                EstimatedDurationMinutes = 120,
            },
            new Service
            {
                DeviceType = "Laptop",
                ServiceName = "Keyboard repair",
                EstimatedPrice = 75,
                EstimatedDurationMinutes = 60,
            },
            new Service
            {
                DeviceType = "Tablet",
                ServiceName = "Charging port repair",
                EstimatedPrice = 65,
                EstimatedDurationMinutes = 60,
            }
        );

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var adminEmail = config["Seed:AdminEmail"] ?? "admin@neofix.local";
        var adminPassword = config["Seed:AdminPassword"] ?? "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                db.Profiles.Add(new Profile
                {
                    Id = admin.Id,
                    FullName = "NeoFix Admin",
                    PhoneNumber = "+380000000000",
                });
                db.AppUserRoles.Add(new UserRole { UserId = admin.Id, Role = AppRole.Admin });
                db.AppUserRoles.Add(new UserRole { UserId = admin.Id, Role = AppRole.User });
            }
        }

        await db.SaveChangesAsync();
    }
}

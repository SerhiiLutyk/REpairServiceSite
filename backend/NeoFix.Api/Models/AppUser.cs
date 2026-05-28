using Microsoft.AspNetCore.Identity;

namespace NeoFix.Api.Models;

public class AppUser : IdentityUser<Guid>
{
    public Profile? Profile { get; set; }
    public ICollection<UserRole> Roles { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}

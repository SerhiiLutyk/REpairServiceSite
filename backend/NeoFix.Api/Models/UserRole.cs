namespace NeoFix.Api.Models;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppRole Role { get; set; } = AppRole.User;

    public AppUser User { get; set; } = null!;
}

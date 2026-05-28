namespace NeoFix.Api.Models;

public class Profile
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;
}

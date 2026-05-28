namespace NeoFix.Api.Models;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeOnly BookingTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? MasterNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;
    public Service Service { get; set; } = null!;
}

namespace NeoFix.Api.Models;

public class Service
{
    public Guid Id { get; set; }
    public string DeviceType { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public decimal EstimatedPrice { get; set; }
    public int EstimatedDurationMinutes { get; set; } = 60;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Booking> Bookings { get; set; } = [];
}

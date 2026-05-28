using System.ComponentModel.DataAnnotations;
using NeoFix.Api.Models;

namespace NeoFix.Api.Dto;

public record BookingServiceDto(string ServiceName, string? DeviceType, decimal EstimatedPrice);

public record BookingDto(
    Guid Id,
    string BookingDate,
    string BookingTime,
    string Status,
    string? MasterNotes,
    BookingServiceDto? Service);

public record AdminBookingDto(
    Guid Id,
    string BookingDate,
    string BookingTime,
    string Status,
    string? MasterNotes,
    Guid UserId,
    BookingServiceDto? Service,
    ProfileSummaryDto? Profile);

public record ProfileSummaryDto(Guid Id, string? FullName, string? PhoneNumber);

public record CreateBookingRequest(
    [Required] Guid ServiceId,
    [Required] string BookingDate,
    [Required] string BookingTime);

public record UpdateBookingStatusRequest([Required] string Status);

public record UpdateMasterNotesRequest(string? MasterNotes);

public record BookingSlotDto(string BookingTime);

public record AnalyticsDto(
    int TotalBookings,
    decimal Revenue,
    int ActiveBookings,
    IReadOnlyList<DailyBookingCountDto> Last7Days);

public record DailyBookingCountDto(string Day, string Date, int Bookings);

public record AnalyticsBookingRow(string BookingDate, string Status, decimal? EstimatedPrice);

public static class BookingStatusMapper
{
    public static string ToApi(BookingStatus status) => status switch
    {
        BookingStatus.InProgress => "In Progress",
        _ => status.ToString(),
    };

    public static bool TryParse(string value, out BookingStatus status)
    {
        status = default;
        if (string.Equals(value, "In Progress", StringComparison.OrdinalIgnoreCase))
        {
            status = BookingStatus.InProgress;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out status);
    }
}

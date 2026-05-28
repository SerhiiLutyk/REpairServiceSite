using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Data;
using NeoFix.Api.Dto;
using NeoFix.Api.Extensions;
using NeoFix.Api.Models;

namespace NeoFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("slots")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<object>>> GetSlots([FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var bookingDate))
            return BadRequest(new { message = "Invalid date. Use yyyy-MM-dd." });

        var times = await db.Bookings.AsNoTracking()
            .Where(b => b.BookingDate == bookingDate && b.Status != BookingStatus.Cancelled)
            .Select(b => b.BookingTime)
            .ToListAsync();

        return Ok(times.Select(t => new { booking_time = t.ToString("HH:mm") + ":00" }));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> MyBookings()
    {
        var userId = User.GetUserId();
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.Service)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.BookingTime)
            .ToListAsync();

        return Ok(rows.Select(MapUserBooking));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create([FromBody] CreateBookingRequest request)
    {
        if (!DateOnly.TryParse(request.BookingDate, out var bookingDate))
            return BadRequest(new { message = "Invalid booking_date." });

        if (!TimeOnly.TryParse(request.BookingTime, out var bookingTime))
            return BadRequest(new { message = "Invalid booking_time." });

        var serviceExists = await db.Services.AnyAsync(s => s.Id == request.ServiceId);
        if (!serviceExists)
            return BadRequest(new { message = "Service not found." });

        var slotTaken = await db.Bookings.AnyAsync(b =>
            b.BookingDate == bookingDate &&
            b.BookingTime == bookingTime &&
            b.Status != BookingStatus.Cancelled);

        if (slotTaken)
            return Conflict(new { message = "This time slot is already booked." });

        var booking = new Booking
        {
            UserId = User.GetUserId(),
            ServiceId = request.ServiceId,
            BookingDate = bookingDate,
            BookingTime = bookingTime,
            Status = BookingStatus.Pending,
        };

        db.Bookings.Add(booking);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "This time slot is already booked." });
        }

        return CreatedAtAction(nameof(MyBookings), null, new { id = booking.Id });
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult> Cancel(Guid id)
    {
        var userId = User.GetUserId();
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (booking is null)
            return NotFound();

        if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            return BadRequest(new { message = "Only pending or confirmed bookings can be cancelled." });

        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<object>>> AdminList()
    {
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.Service)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.BookingTime)
            .ToListAsync();

        var userIds = rows.Select(r => r.UserId).Distinct().ToList();
        var profiles = await db.Profiles.AsNoTracking()
            .Where(p => userIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        return Ok(rows.Select(b =>
        {
            profiles.TryGetValue(b.UserId, out var profile);
            return new
            {
                id = b.Id,
                booking_date = b.BookingDate.ToString("yyyy-MM-dd"),
                booking_time = b.BookingTime.ToString("HH:mm:ss"),
                status = BookingStatusMapper.ToApi(b.Status),
                master_notes = b.MasterNotes,
                user_id = b.UserId,
                service = b.Service is null ? null : new
                {
                    service_name = b.Service.ServiceName,
                    estimated_price = b.Service.EstimatedPrice,
                },
                profile = profile is null ? null : new
                {
                    id = profile.Id,
                    full_name = profile.FullName,
                    phone_number = profile.PhoneNumber,
                },
            };
        }));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusRequest request)
    {
        if (!BookingStatusMapper.TryParse(request.Status, out var status))
            return BadRequest(new { message = "Invalid status." });

        var booking = await db.Bookings.FindAsync(id);
        if (booking is null)
            return NotFound();

        booking.Status = status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/notes")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateNotes(Guid id, [FromBody] UpdateMasterNotesRequest request)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking is null)
            return NotFound();

        booking.MasterNotes = request.MasterNotes;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static object MapUserBooking(Booking b) => new
    {
        id = b.Id,
        booking_date = b.BookingDate.ToString("yyyy-MM-dd"),
        booking_time = b.BookingTime.ToString("HH:mm:ss"),
        status = BookingStatusMapper.ToApi(b.Status),
        master_notes = b.MasterNotes,
        service = b.Service is null ? null : new
        {
            service_name = b.Service.ServiceName,
            device_type = b.Service.DeviceType,
            estimated_price = b.Service.EstimatedPrice,
        },
    };
}

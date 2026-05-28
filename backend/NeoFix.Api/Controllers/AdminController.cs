using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Data;
using NeoFix.Api.Dto;
using NeoFix.Api.Models;

namespace NeoFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsDto>> Analytics()
    {
        var rows = await db.Bookings.AsNoTracking()
            .Include(b => b.Service)
            .Select(b => new
            {
                b.BookingDate,
                b.Status,
                Price = b.Service != null ? b.Service.EstimatedPrice : 0m,
            })
            .ToListAsync();

        var total = rows.Count;
        var revenue = rows
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Sum(b => b.Price);

        var active = rows.Count(b =>
            b.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.InProgress);

        var last7 = Enumerable.Range(0, 7)
            .Select(i => DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-6 + i)))
            .Select(d => new DailyBookingCountDto(
                d.ToString("ddd"),
                d.ToString("yyyy-MM-dd"),
                rows.Count(b => b.BookingDate == d)))
            .ToList();

        return Ok(new AnalyticsDto(total, revenue, active, last7));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Data;
using NeoFix.Api.Dto;
using NeoFix.Api.Models;

namespace NeoFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<object>>> List([FromQuery] bool summary = false)
    {
        var query = db.Services.AsNoTracking().OrderBy(s => s.DeviceType);

        if (summary)
        {
            var items = await query
                .Select(s => new ServiceListItemDto(s.Id, s.DeviceType, s.ServiceName, s.EstimatedPrice))
                .ToListAsync();
            return Ok(items.Select(ToSnakeCaseSummary));
        }

        var full = await query.ToListAsync();
        return Ok(full.Select(ToSnakeCaseFull));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Get(Guid id)
    {
        var service = await db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (service is null)
            return NotFound();
        return Ok(ToSnakeCaseFull(service));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> Create([FromBody] UpsertServiceRequest request)
    {
        var service = new Service
        {
            DeviceType = request.DeviceType,
            ServiceName = request.ServiceName,
            EstimatedPrice = request.EstimatedPrice,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
        };
        db.Services.Add(service);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = service.Id }, ToSnakeCaseFull(service));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpsertServiceRequest request)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null)
            return NotFound();

        service.DeviceType = request.DeviceType;
        service.ServiceName = request.ServiceName;
        service.EstimatedPrice = request.EstimatedPrice;
        service.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null)
            return NotFound();

        db.Services.Remove(service);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static object ToSnakeCaseFull(Service s) => new
    {
        id = s.Id,
        device_type = s.DeviceType,
        service_name = s.ServiceName,
        estimated_price = s.EstimatedPrice,
        estimated_duration_minutes = s.EstimatedDurationMinutes,
        created_at = s.CreatedAt,
    };

    private static object ToSnakeCaseSummary(ServiceListItemDto s) => new
    {
        id = s.Id,
        device_type = s.DeviceType,
        service_name = s.ServiceName,
        estimated_price = s.EstimatedPrice,
    };
}

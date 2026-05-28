using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Data;
using NeoFix.Api.Dto;
using NeoFix.Api.Extensions;

namespace NeoFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<object>> GetMe()
    {
        var userId = User.GetUserId();
        var profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
        return Ok(new
        {
            id = userId,
            full_name = profile?.FullName,
            phone_number = profile?.PhoneNumber,
        });
    }

    [HttpPut("me")]
    public async Task<ActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var userId = User.GetUserId();
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == userId);
        if (profile is null)
        {
            profile = new Models.Profile { Id = userId };
            db.Profiles.Add(profile);
        }

        if (request.FullName is not null)
            profile.FullName = request.FullName;
        if (request.PhoneNumber is not null)
            profile.PhoneNumber = request.PhoneNumber;

        await db.SaveChangesAsync();
        return NoContent();
    }
}

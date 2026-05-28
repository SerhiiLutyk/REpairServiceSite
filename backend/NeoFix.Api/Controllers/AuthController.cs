using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoFix.Api.Data;
using NeoFix.Api.Dto;
using NeoFix.Api.Extensions;
using NeoFix.Api.Models;
using NeoFix.Api.Services;

namespace NeoFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<AppUser> userManager,
    ApplicationDbContext db,
    TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { message = "Email is already registered." });

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });

        db.Profiles.Add(new Profile
        {
            Id = user.Id,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
        });
        db.AppUserRoles.Add(new UserRole { UserId = user.Id, Role = AppRole.User });
        await db.SaveChangesAsync();

        return Ok(await BuildAuthResponse(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(await BuildAuthResponse(user));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = User.GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Unauthorized();

        var profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
        var isAdmin = await db.AppUserRoles.AnyAsync(r => r.UserId == userId && r.Role == AppRole.Admin);

        return Ok(new UserDto(user.Id, user.Email ?? "", isAdmin, profile?.FullName, profile?.PhoneNumber));
    }

    private async Task<AuthResponse> BuildAuthResponse(AppUser user)
    {
        var roles = await db.AppUserRoles
            .Where(r => r.UserId == user.Id)
            .Select(r => r.Role)
            .ToListAsync();

        var profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == user.Id);
        var token = tokenService.CreateToken(user, roles);
        var expires = DateTime.UtcNow.AddDays(7);

        return new AuthResponse(
            token,
            expires,
            new UserDto(
                user.Id,
                user.Email ?? "",
                roles.Contains(AppRole.Admin),
                profile?.FullName,
                profile?.PhoneNumber));
    }
}

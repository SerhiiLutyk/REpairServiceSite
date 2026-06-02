using System.Security.Claims;
using GadgetFix.Users.BLL.DTOs;
using GadgetFix.Users.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService users, ITokenService tokens) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IReadOnlyList<UserDto>> GetAll(CancellationToken ct) =>
        await users.GetAllAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Профіль поточного користувача (за JWT).</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        if (!TryGetUserId(out var guid)) return Unauthorized();
        var user = await users.GetByIdAsync(guid, ct);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Оновлення профілю поточного користувача.</summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var guid)) return Unauthorized();
        var user = await users.UpdateProfileAsync(guid, request, ct);
        return user is null ? NotFound() : Ok(user);
    }

    private bool TryGetUserId(out Guid id)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out id);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var user = await users.RegisterAsync(request, ct);
            return Ok(new AuthResponse(tokens.CreateToken(user), user));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await users.LoginAsync(request, ct);
        return user is null
            ? Unauthorized(new { error = "Невірний телефон або пароль." })
            : Ok(new AuthResponse(tokens.CreateToken(user), user));
    }

    /// <summary>Згенерувати код для прив'язки Telegram (показується в кабінеті).</summary>
    [Authorize]
    [HttpPost("me/telegram-code")]
    public async Task<ActionResult> GenerateTelegramCode(CancellationToken ct)
    {
        if (!TryGetUserId(out var guid)) return Unauthorized();
        var code = await users.GenerateLinkCodeAsync(guid, ct);
        return Ok(new { code });
    }

    // ---- Внутрішні ендпоінти для Telegram-бота (не проксуються через gateway) ----

    [HttpPost("/internal/users/telegram-link")]
    public async Task<ActionResult<UserDto>> LinkTelegram(LinkTelegramRequest request, CancellationToken ct)
    {
        var user = await users.LinkTelegramAsync(request.Code, request.ChatId, ct);
        return user is null ? NotFound(new { error = "Невірний код." }) : Ok(user);
    }

    [HttpGet("/internal/users/by-telegram/{chatId}")]
    public async Task<ActionResult<UserDto>> GetByTelegram(string chatId, CancellationToken ct)
    {
        var user = await users.GetByTelegramAsync(chatId, ct);
        return user is null ? NotFound() : Ok(user);
    }
}

public record LinkTelegramRequest(string Code, string ChatId);

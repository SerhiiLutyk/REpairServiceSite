using GadgetFix.Users.BLL.DTOs;
using GadgetFix.Users.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService users) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<UserDto>> GetAll(CancellationToken ct) =>
        await users.GetAllAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var user = await users.RegisterAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await users.LoginAsync(request, ct);
        return user is null ? Unauthorized(new { error = "Невірний телефон або пароль." }) : Ok(user);
    }
}

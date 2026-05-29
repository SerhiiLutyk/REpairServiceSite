using GadgetFix.Users.BLL.DTOs;
using GadgetFix.Users.DAL;
using GadgetFix.Users.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Users.BLL.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<UserDto?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public class UserService(UsersDbContext db) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default) =>
        await db.Users.AsNoTracking().OrderBy(u => u.CreatedAt)
            .Select(u => UserDto.From(u)).ToListAsync(ct);

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        return user is null ? null : UserDto.From(user);
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(u => u.Phone == request.Phone, ct))
            throw new InvalidOperationException("Користувач з таким телефоном вже існує.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return UserDto.From(user);
    }

    public async Task<UserDto?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;
        return UserDto.From(user);
    }
}

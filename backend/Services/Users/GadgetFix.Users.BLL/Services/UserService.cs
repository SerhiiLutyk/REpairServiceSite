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
    Task<UserDto?> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default);
    Task<string> GenerateLinkCodeAsync(Guid id, CancellationToken ct = default);
    Task<UserDto?> LinkTelegramAsync(string code, string chatId, CancellationToken ct = default);
    Task<UserDto?> GetByTelegramAsync(string chatId, CancellationToken ct = default);
    Task EnsureAdminAsync(string fullName, string phone, string password, CancellationToken ct = default);
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
        var login = request.Login.Trim();
        var loginLower = login.ToLower();
        // Вхід за телефоном АБО email
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Phone == login || (u.Email != null && u.Email.ToLower() == loginLower), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;
        return UserDto.From(user);
    }

    public async Task<UserDto?> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return null;

        user.FullName = request.FullName.Trim();
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.TelegramChatId = string.IsNullOrWhiteSpace(request.TelegramChatId) ? null : request.TelegramChatId.Trim();
        await db.SaveChangesAsync(ct);
        return UserDto.From(user);
    }

    public async Task<string> GenerateLinkCodeAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new InvalidOperationException("Користувача не знайдено.");

        var code = "GF-" + Random.Shared.Next(100000, 999999);
        user.TelegramLinkCode = code;
        await db.SaveChangesAsync(ct);
        return code;
    }

    public async Task<UserDto?> LinkTelegramAsync(string code, string chatId, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramLinkCode == code, ct);
        if (user is null) return null;

        user.TelegramChatId = chatId;
        user.TelegramLinkCode = null;
        await db.SaveChangesAsync(ct);
        return UserDto.From(user);
    }

    public async Task<UserDto?> GetByTelegramAsync(string chatId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
        return user is null ? null : UserDto.From(user);
    }

    public async Task EnsureAdminAsync(string fullName, string phone, string password, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(u => u.Phone == phone, ct))
            return;

        db.Users.Add(new User
        {
            FullName = fullName,
            Phone = phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Admin,
        });
        await db.SaveChangesAsync(ct);
    }
}

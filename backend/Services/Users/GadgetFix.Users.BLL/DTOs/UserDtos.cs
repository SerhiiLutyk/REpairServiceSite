using GadgetFix.Users.DAL.Entities;

namespace GadgetFix.Users.BLL.DTOs;

public record RegisterRequest(string FullName, string Phone, string? Email, string Password);

public record LoginRequest(string Phone, string Password);

public record UpdateProfileRequest(string FullName, string? Email, string? TelegramChatId);

public record UserDto(Guid Id, string FullName, string Phone, string? Email, string? TelegramChatId, UserRole Role, DateTime CreatedAt)
{
    public static UserDto From(User u) => new(u.Id, u.FullName, u.Phone, u.Email, u.TelegramChatId, u.Role, u.CreatedAt);
}

public record AuthResponse(string Token, UserDto User);

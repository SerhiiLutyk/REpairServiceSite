using System.ComponentModel.DataAnnotations;

namespace NeoFix.Api.Dto;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required, MinLength(2), MaxLength(100)] string FullName,
    [Required, MinLength(5), MaxLength(30)] string PhoneNumber);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    Guid Id,
    string Email,
    bool IsAdmin,
    string? FullName,
    string? PhoneNumber);

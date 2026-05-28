using System.ComponentModel.DataAnnotations;

namespace NeoFix.Api.Dto;

public record ReviewDto(
    Guid Id,
    int Rating,
    string? Comment,
    DateTime CreatedAt,
    Guid UserId);

public record CreateReviewRequest(
    [Range(1, 5)] int Rating,
    [MaxLength(2000)] string? Comment);

public record UpdateProfileRequest(
    [MinLength(2), MaxLength(100)] string? FullName,
    [MinLength(5), MaxLength(30)] string? PhoneNumber);

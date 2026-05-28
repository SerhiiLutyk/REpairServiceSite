using System.ComponentModel.DataAnnotations;

namespace NeoFix.Api.Dto;

public record ServiceDto(
    Guid Id,
    string DeviceType,
    string ServiceName,
    decimal EstimatedPrice,
    int EstimatedDurationMinutes,
    DateTime CreatedAt);

public record ServiceListItemDto(
    Guid Id,
    string DeviceType,
    string ServiceName,
    decimal EstimatedPrice);

public record UpsertServiceRequest(
    [Required] string DeviceType,
    [Required] string ServiceName,
    [Range(0, 999999)] decimal EstimatedPrice,
    [Range(1, 1440)] int EstimatedDurationMinutes = 60);

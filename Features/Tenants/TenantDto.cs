using System.ComponentModel.DataAnnotations;
using HouseRentMgmt.Api.Infrastructure.Validation;
namespace HouseRentMgmt.Api.Features.Tenants;

public record AddTenantRequestDto(
    string Name,
    string Phone,
    string? Email)
{
    [Required(ErrorMessage = "Tenant name is required")]
    [MaxLength(100, ErrorMessage = "Tenant name is too long")]
    public string Name { get; init; } = Name?.Trim()!;
    [Required(ErrorMessage = "Invalid Nepali mobile number format")]
    [RegularExpression(RequestPatterns.Phone, ErrorMessage = "Invalid Nepali mobile number format")]
    public string Phone { get; init; } = Phone?.Trim()!;
    [RegularExpression(RequestPatterns.Email, ErrorMessage = "Invalid email address")]
    public string? Email { get; init; } = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim().ToLowerInvariant();
}


public record TenantResponseDto
(
    bool Success,
    string Message
);

public record TenantDetailsDto 
(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    DateTime CreatedAt
);
public record EditTenantRequestDto(
    string Name,
    string Phone,
    string? Email)
{
    [Required(ErrorMessage = "Tenant name is required")]
    [MaxLength(100, ErrorMessage = "Tenant name is too long")]
    public string Name { get; init; } = Name?.Trim()!;
    [Required(ErrorMessage = "Invalid Nepali mobile number format")]
    [RegularExpression(RequestPatterns.Phone, ErrorMessage = "Invalid Nepali mobile number format")]
    public string Phone { get; init; } = Phone?.Trim()!;
    [RegularExpression(RequestPatterns.Email, ErrorMessage = "Invalid email address")]
    public string? Email { get; init; } = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim().ToLowerInvariant();
}


public record TenantRoomDto(Guid Id, string RoomName, string FloorId);

public record TenantLeaseDto(
    Guid Id,
    decimal MonthlyRent,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    TenantRoomDto Room);

public record TenantListItemDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    IReadOnlyList<TenantLeaseDto> ActiveLeases);

public record TenantListResponseDto(bool Success, IReadOnlyList<TenantListItemDto> Tenants);

public record TenantProfileDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<TenantLeaseDto> Leases);

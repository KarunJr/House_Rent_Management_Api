using System.ComponentModel.DataAnnotations;
using HouseRentMgmt.Api.Infrastructure.Validation;
namespace HouseRentMgmt.Api.Features.Leases;

public record AddLeaseRequestDto(
    Guid RoomId,
    Guid TenantId,
    DateOnly StartDate,
    decimal MonthlyRent)
{
    [NonDefault(ErrorMessage = "Select a room")]
    public Guid RoomId { get; init; } = RoomId;
    [NonDefault(ErrorMessage = "Select a tenant")]
    public Guid TenantId { get; init; } = TenantId;
    [NonDefault(ErrorMessage = "Start date is required")]
    public DateOnly StartDate { get; init; } = StartDate;
    [Range(typeof(decimal), "0", "9999999", MinimumIsExclusive = true, ErrorMessage = "Monthly rent must be greater than 0 and no more than 9999999")]
    public decimal MonthlyRent { get; init; } = MonthlyRent;
}


public record LeaseResponseDto(bool Success, string Message);

public record LeaseDetailsDto(
    Guid Id,
    Guid RoomId,
    Guid TenantId,
    decimal MonthlyRent,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateTime CreatedAt);

public record EndLeaseDto(
    DateOnly EndDate)
{
    [NonDefault(ErrorMessage = "Move-out date is required")]
    public DateOnly EndDate { get; init; } = EndDate;
}

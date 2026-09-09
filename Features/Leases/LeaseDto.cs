namespace HouseRentMgmt.Api.Features.Leases;

public record AddLeaseRequestDto(
    Guid RoomId,
    Guid TenantId,
    DateOnly StartDate,
    decimal MonthlyRent);

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
    DateOnly EndDate
);

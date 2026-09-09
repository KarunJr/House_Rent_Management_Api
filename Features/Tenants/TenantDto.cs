namespace HouseRentMgmt.Api.Features.Tenants;

public record AddTenantRequestDto
(
    string Name,
    string Phone,
    string? Email
);

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
public record EditTenantRequestDto 
(
    string Name,
    string Phone,
    string? Email
);

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

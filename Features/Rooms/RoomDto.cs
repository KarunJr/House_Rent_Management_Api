using HouseRentMgmt.Api.Features.Rooms.Entities;

namespace HouseRentMgmt.Api.Features.Rooms;

public record RoomRequestDto
(
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status
);
public record EditRoomRequestDto
(
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status
);
public record RoomResponseDto
(
    bool Success,
    string Message
);

public record RoomDetailsDto
(
    Guid Id,
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status,
    DateTime CreatedAt
);

public record RoomCardDto
(
    Guid Id,
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status,
    RoomActiveLeaseDto? ActiveLease
);

public record RoomActiveLeaseDto
(
    Guid Id,
    decimal MonthlyRent,
    DateOnly StartDate,
    DateOnly? EndDate,
    RoomTenantDto Tenant
);

public record RoomTenantDto(Guid Id, string Name);

public record RoomListResponseDto(bool Success, IReadOnlyList<RoomCardDto> Rooms);

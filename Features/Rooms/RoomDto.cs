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
    Guid Id,
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

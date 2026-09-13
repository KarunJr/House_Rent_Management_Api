using System.ComponentModel.DataAnnotations;
using HouseRentMgmt.Api.Features.Rooms.Entities;

namespace HouseRentMgmt.Api.Features.Rooms;

public record RoomRequestDto(
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status)
{
    [Required(ErrorMessage = "Select a floor")]
    [RegularExpression(@"0*[1-9][0-9]*", ErrorMessage = "Select a floor")]
    public string FloorId { get; init; } = FloorId?.Trim()!;
    [Required(ErrorMessage = "Room name is required")]
    [MaxLength(20, ErrorMessage = "Room name is too long")]
    [RegularExpression(@"[A-Za-z0-9-]+", ErrorMessage = "Use only letters, numbers, or hyphen")]
    public string RoomName { get; init; } = RoomName?.Trim()!;
    [Range(typeof(decimal), "0", "9999999", MinimumIsExclusive = true, ErrorMessage = "Base rent must be greater than 0 and no more than 9999999")]
    public decimal BaseRentAmount { get; init; } = BaseRentAmount;
    [EnumDataType(typeof(RoomStatus), ErrorMessage = "Select a valid room status")]
    [System.Text.Json.Serialization.JsonRequired]
    public RoomStatus Status { get; init; } = Status;
}

public record EditRoomRequestDto(
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status)
{
    [Required(ErrorMessage = "Select a floor")]
    [RegularExpression(@"0*[1-9][0-9]*", ErrorMessage = "Select a floor")]
    public string FloorId { get; init; } = FloorId?.Trim()!;
    [Required(ErrorMessage = "Room name is required")]
    [MaxLength(20, ErrorMessage = "Room name is too long")]
    [RegularExpression(@"[A-Za-z0-9-]+", ErrorMessage = "Use only letters, numbers, or hyphen")]
    public string RoomName { get; init; } = RoomName?.Trim()!;
    [Range(typeof(decimal), "0", "9999999", MinimumIsExclusive = true, ErrorMessage = "Base rent must be greater than 0 and no more than 9999999")]
    public decimal BaseRentAmount { get; init; } = BaseRentAmount;
    [EnumDataType(typeof(RoomStatus), ErrorMessage = "Select a valid room status")]
    [System.Text.Json.Serialization.JsonRequired]
    public RoomStatus Status { get; init; } = Status;
}

public record RoomDetails
(
    Guid Id,
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status,
    DateTime CreatedAt
);
public record RoomResponseDto
(
    bool Success,
    string Message,
    RoomDetails? RoomDetails
);

public record SingleRoomResponseDto(
    bool Success,
    string Message,
    RoomCardDto? RoomDetails
);

public record RoomCardDto
(
    Guid Id,
    string FloorId,
    string RoomName,
    decimal BaseRentAmount,
    RoomStatus Status,
    bool HasLease,
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

using HouseRentMgmt.Api.Features.Leases.Entities;

namespace HouseRentMgmt.Api.Features.Rooms.Entities;

public class Room
{
    public Guid Id {get; set;}
    public required string FloorId {get; set;}
    public required string RoomName {get; set;}
    public decimal BaseRentAmount {get; set;}
    public RoomStatus Status {get; set;} = RoomStatus.Available;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public ICollection<Lease> Leases {get; set;} = [];
}

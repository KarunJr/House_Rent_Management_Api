using HouseRentMgmt.Api.Features.Rooms.Entities;
using HouseRentMgmt.Api.Features.Tenants.Entities;

namespace HouseRentMgmt.Api.Features.Leases.Entities;

public class Lease
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid TenantId {get; set;}
    public Room Room {get; set;} = null!;
    public Tenant Tenant {get; set;} = null!;
    public decimal MonthlyRent { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

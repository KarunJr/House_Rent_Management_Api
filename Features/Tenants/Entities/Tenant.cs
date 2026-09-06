using HouseRentMgmt.Api.Features.Leases.Entities;

namespace HouseRentMgmt.Api.Features.Tenants.Entities;

public class Tenant
{
    public Guid Id {get; set;}
    public required string Name {get; set;}
    public required string Phone {get; set;}
    public string? Email {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;
    public ICollection<Lease> Leases {get; set;} = [];
}

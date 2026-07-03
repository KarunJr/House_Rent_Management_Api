using Microsoft.AspNetCore.Identity;

namespace HouseRentMgmt.Api.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string Name {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}
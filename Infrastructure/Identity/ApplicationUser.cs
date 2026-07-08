using Microsoft.AspNetCore.Identity;

namespace HouseRentMgmt.Api.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string Name {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}

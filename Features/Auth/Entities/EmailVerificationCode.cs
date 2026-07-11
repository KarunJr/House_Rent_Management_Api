using HouseRentMgmt.Api.Infrastructure.Identity;

namespace HouseRentMgmt.Api.Features.Auth.Entities;

public class  EmailVerificationCode
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public ApplicationUser User {get; set;} = null!;
    public required string OtpCode {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime ExpiresAt {get; set;} = DateTime.UtcNow.AddMinutes(10);
    public DateTime? UsedAt {get; set;}
    public int AttemptCount {get; set;}
}

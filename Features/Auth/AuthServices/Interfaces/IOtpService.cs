using HouseRentMgmt.Api.Features.Auth.Entities;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

public enum OtpVerificationResult
{
    Success,
    InvalidCode,
    Expired,
    TooManyAttempts,
    AlreadyUsed
}
public interface IOtpService
{
    string GenerateOtp();

    OtpVerificationResult VerifyOtp(string code, EmailVerificationCode dbOtp);

}

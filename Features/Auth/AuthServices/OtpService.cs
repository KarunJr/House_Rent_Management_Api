using System.Security.Cryptography;
using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
using HouseRentMgmt.Api.Features.Auth.Entities;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices;


public class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        int randomNumber = RandomNumberGenerator.GetInt32(100000, 1000000);
        return randomNumber.ToString("D6");
    }

    public OtpVerificationResult VerifyOtp(string userOtp, EmailVerificationCode dbOtp)
    {
        if (dbOtp.UsedAt != null)
        {
            return OtpVerificationResult.AlreadyUsed;
        }

        if (dbOtp.AttemptCount >= 5)
        {
            return OtpVerificationResult.TooManyAttempts;
        }

        if (dbOtp.ExpiresAt <= DateTime.UtcNow)
        {
            return OtpVerificationResult.Expired;
        }

        if (userOtp != dbOtp.OtpCode)
        {
            dbOtp.AttemptCount++;
            return OtpVerificationResult.InvalidCode;
        }

        dbOtp.UsedAt = DateTime.UtcNow;

        return OtpVerificationResult.Success;
    }
}

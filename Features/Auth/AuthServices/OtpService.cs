using System.Security.Cryptography;
using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices;

public class OtpService: IOtpService
{
    public string GenerateOtp()
    {
        int randomNumber = RandomNumberGenerator.GetInt32(100000, 1000000);
        return randomNumber.ToString("D6");
    }

    public bool VerifyOtp(string code)
    {
        return false;
    }
}

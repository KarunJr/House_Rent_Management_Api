namespace HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

public interface IOtpService
{
    string GenerateOtp();

    bool VerifyOtp(string code);


}

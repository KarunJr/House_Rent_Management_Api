

using HouseRentMgmt.Api.Features.Auth;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

public interface ITokenService
{
    string GenerateToken(TokenUserDto user);
}

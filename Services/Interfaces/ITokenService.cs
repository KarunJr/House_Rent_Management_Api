using HouseRentMgmt.Api.DTO;

namespace HouseRentMgmt.Api.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(TokenUserDto user);
}

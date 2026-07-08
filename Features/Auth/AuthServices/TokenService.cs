using System.Security.Claims;
using System.Text;
using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices;

public class TokenService(IConfiguration config) : ITokenService
{
    public string GenerateToken(TokenUserDto user)
    {
        var secretKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Missing configuration: 'Jwt:Key'.");
        var issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Missing configuration: 'Jwt:Issuer'.");
        var audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Missing configuration: 'Jwt:Audience'.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("display_name", user.Name)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JsonWebTokenHandler();
        string token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
}



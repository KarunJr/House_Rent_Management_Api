using System.Text;
using HouseRentMgmt.Api.Services;
using HouseRentMgmt.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HouseRentMgmt.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCorsService(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("NextJsPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });
        return services;
    }

    public static IServiceCollection AddAuthenticationService(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
            };
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddTokenService(this IServiceCollection service)
    {   
        service.AddScoped<ITokenService, TokenService>();

        return service;
    }
    // public static IServiceCollection AddCustomService(this IServiceCollection services)
    // {
    //     return services;
    // }
}

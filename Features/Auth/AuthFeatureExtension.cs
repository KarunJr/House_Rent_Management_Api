using HouseRentMgmt.Api.Features.Auth.AuthHandler;
using HouseRentMgmt.Api.Features.Auth.AuthServices;
using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

namespace HouseRentMgmt.Api.Features.Auth;

public static class AuthFeatureExtension
{
    public static IServiceCollection AddAuthFeatureService(this IServiceCollection service)
    {
        service.AddScoped<ITokenService, TokenService>();
        service.AddScoped<ICookieService, CookieService>();

        return service;
    }

    public static IEndpointRouteBuilder MapAuthEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");
        group.MapRegister();

        return app;
    }
}

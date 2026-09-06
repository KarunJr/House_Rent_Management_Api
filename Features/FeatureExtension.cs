using HouseRentMgmt.Api.Features.Auth;
using HouseRentMgmt.Api.Features.Rooms;

namespace HouseRentMgmt.Api.Features;

public static class FeatureExtension
{
    public static IServiceCollection AddAllFeatureServices(this IServiceCollection services)
    {
        services.AddAuthFeatureService();
        return services;
    }

    public static void MapAllApplicationEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndPoints();
        app.MapRoomsEndpoints();
    }
}

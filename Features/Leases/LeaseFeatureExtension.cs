using HouseRentMgmt.Api.Features.Leases.LeaseHandler;

namespace HouseRentMgmt.Api.Features.Leases;

public static class LeaseFeatureExtension
{
    public static IEndpointRouteBuilder MapLeaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/lease").RequireAuthorization();
        group.MapAddLease();
        group.MapEndLease();
        return app;
    }
}

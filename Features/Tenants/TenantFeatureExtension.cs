using HouseRentMgmt.Api.Features.Tenants.TenantHandler;

namespace HouseRentMgmt.Api.Features.Tenants;

public static class TenantFeatureExtension
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenant").RequireAuthorization();
        group.MapAddTenant();
        group.MapEditTenant();
        group.MapGetTenant();
        group.MapGetTenantById();

        return app;
    }
}

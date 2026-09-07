using HouseRentMgmt.Api.Features.Rooms.RoomHandler;

namespace HouseRentMgmt.Api.Features.Rooms;

public static class RoomsFeatureExtension
{
    public static IEndpointRouteBuilder MapRoomsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/room").RequireAuthorization();
        group.MapAddRoom();
        group.MapEditRoom();

        return app;
    }
}

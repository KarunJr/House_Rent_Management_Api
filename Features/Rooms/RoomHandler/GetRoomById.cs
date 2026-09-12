using System.Security.Claims;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Features.Rooms.RoomHandler;

public static class GetRoomById
{
    public static void MapGetRoomById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        ApplicationDbContext dbContext,
        ClaimsPrincipal user,
        ILoggerFactory loggerFactory,
        Guid id
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(GetRoomById));
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Room lookup rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        var room = await dbContext.Room
            .AsNoTracking()
            .Where(r => r.Id == id && r.UserId == userId)
            .Select(r => new RoomCardDto(
                Id: r.Id,
                FloorId: r.FloorId,
                RoomName: r.RoomName,
                BaseRentAmount: r.BaseRentAmount,
                Status: r.Status,
                ActiveLease: r.Leases.Where(l => l.IsActive)
                                    .Select(l => new RoomActiveLeaseDto(
                                        Id: l.Id,
                                        MonthlyRent: l.MonthlyRent,
                                        StartDate: l.StartDate,
                                        EndDate: l.EndDate,
                                        Tenant: new RoomTenantDto(l.TenantId, l.Tenant.Name)
                                    )).FirstOrDefault()

            )
            ).FirstOrDefaultAsync();
        
        if(room is null)
        {
            return Results.NotFound(new RoomResponseDto(false, "Room not found.", null));
        }
        return Results.Ok(room);
    }
}

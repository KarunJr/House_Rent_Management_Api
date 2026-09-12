using System.Security.Claims;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Features.Rooms.RoomHandler;

public static class GetRoom
{
    public static void MapGetRoom(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        ApplicationDbContext dbContext,
        ClaimsPrincipal user,
        ILoggerFactory loggerFactory
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(GetRoom));
        logger.LogInformation("Getting the rooms from the db");
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Room listing rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        var rooms = await dbContext.Room
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.RoomName)
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
            ).ToListAsync();
        return Results.Ok(new RoomListResponseDto(Success: true, Rooms: rooms));
    }
}

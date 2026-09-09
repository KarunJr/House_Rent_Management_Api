using System.Security.Claims;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Features.Tenants.TenantHandler;

public static class GetTenant
{
    public static void MapGetTenant(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync);
    }

    public static async Task<IResult> HandleAsync(
        ApplicationDbContext dbContext,
        ClaimsPrincipal user,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(GetTenant));
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Tenant lookup rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        var result = await dbContext.Tenant
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
            .Select(t => new TenantListItemDto(
                t.Id,
                t.Name,
                t.Phone,
                t.Email,
                t.Leases
                    .Where(l => l.IsActive && l.Room.UserId == userId)
                    .OrderByDescending(l => l.StartDate)
                    .ThenBy(l => l.Id)
                    .Select(l => new TenantLeaseDto(
                        l.Id,
                        l.MonthlyRent,
                        l.StartDate,
                        l.EndDate,
                        l.IsActive,
                        new TenantRoomDto(l.Room.Id, l.Room.RoomName, l.Room.FloorId)))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Results.Ok(new TenantListResponseDto(true, result));
    }
}

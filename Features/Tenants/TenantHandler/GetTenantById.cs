using System.Security.Claims;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Features.Tenants.TenantHandler;

public static class GetTenantById
{
    public static void MapGetTenantById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", HandleAsync);
    }

    public static async Task<IResult> HandleAsync(
        Guid id,
        ApplicationDbContext dbContext,
        ClaimsPrincipal user,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(GetTenantById));
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Tenant lookup rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        var result = await dbContext.Tenant
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TenantProfileDto(
                t.Id,
                t.Name,
                t.Phone,
                t.Email,
                t.CreatedAt,
                t.UpdatedAt,
                t.Leases
                    .Where(l => l.Room.UserId == userId)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return Results.NotFound(new TenantResponseDto(false, "Tenant not found."));
        }

        return Results.Ok(result);
    }
}

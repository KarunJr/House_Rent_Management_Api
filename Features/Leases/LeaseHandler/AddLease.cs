using System.Data;
using System.Security.Claims;
using HouseRentMgmt.Api.Features.Leases.Entities;
using HouseRentMgmt.Api.Features.Rooms.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseRentMgmt.Api.Features.Leases.LeaseHandler;

public static class AddLease
{
    public static void MapAddLease(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync);
    }

    public static async Task<IResult> HandleAsync(
        AddLeaseRequestDto request,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(AddLease));
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Lease creation rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        // The overlap check and insert must share a serializable transaction,
        // including for inactive future reservations that the active index cannot protect.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        Lease lease;
        try
        {
            var room = await dbContext.Room.FirstOrDefaultAsync(
                r => r.Id == request.RoomId && r.UserId == userId, cancellationToken);
            if (room is null)
                return Results.NotFound(new LeaseResponseDto(false, "Room not found."));

            var tenantExists = await dbContext.Tenant.AnyAsync(
                t => t.Id == request.TenantId && t.UserId == userId, cancellationToken);
            if (!tenantExists)
                return Results.NotFound(new LeaseResponseDto(false, "Tenant not found."));

            if (room.Status != RoomStatus.Available)
                return Results.Conflict(new LeaseResponseDto(false, "The room is not available for a new lease."));

            // New leases are open-ended. Treat existing end dates as inclusive.
            // Check inactive reservations too, not only currently active leases.
            var hasConflict = await dbContext.Lease.AnyAsync(l =>
                l.RoomId == room.Id &&
                (l.IsActive || l.EndDate == null || l.EndDate >= request.StartDate),
                cancellationToken);
            if (hasConflict)
                return Results.Conflict(new LeaseResponseDto(false, "The room already has an active or overlapping lease."));

            var now = DateTime.UtcNow;
            lease = new Lease
            {
                RoomId = room.Id,
                TenantId = request.TenantId,
                MonthlyRent = request.MonthlyRent,
                StartDate = request.StartDate,
                EndDate = null,
                IsActive = request.StartDate <= DateOnly.FromDateTime(now),
                CreatedAt = now
            };

            if (lease.IsActive)
                room.Status = RoomStatus.Occupied;

            dbContext.Lease.Add(lease);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "unique_active_lease"
        })
        {
            logger.LogWarning("Lease creation rejected: the room already has an active lease.");
            return Results.Conflict(new LeaseResponseDto(false, "The room already has an active lease."));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: "FK_Lease_Room_RoomId" or "FK_Lease_Tenant_TenantId"
        })
        {
            logger.LogWarning("Lease creation rejected: the selected room or tenant no longer exists.");
            return Results.Conflict(new LeaseResponseDto(false, "The room or tenant no longer exists. Reload and try again."));
        }
        catch (Exception ex) when (ex is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure }
            || ex is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.SerializationFailure } })
        {
            logger.LogWarning("Lease creation conflicted with another database transaction.");
            return Results.Conflict(new LeaseResponseDto(false, "Availability changed while saving. Reload and try again."));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Lease {LeaseId} created for room {RoomId} and tenant {TenantId}; active: {IsActive}.",
                lease.Id, lease.RoomId, lease.TenantId, lease.IsActive);
        }

        var response = new LeaseDetailsDto(lease.Id, lease.RoomId, lease.TenantId,
            lease.MonthlyRent, lease.StartDate, lease.EndDate, lease.IsActive, lease.CreatedAt);
        return Results.Created($"/webservice/v1/api/lease/{lease.Id}", response);
    }
}

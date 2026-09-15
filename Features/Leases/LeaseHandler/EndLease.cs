using System.Data;
using System.Security.Claims;
using HouseRentMgmt.Api.Features.Leases.Entities;
using HouseRentMgmt.Api.Features.Rooms.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseRentMgmt.Api.Features.Leases.LeaseHandler;

public static class EndLease
{
    public static void MapEndLease(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:guid}/end", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        EndLeaseDto request,
        Guid id,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(EndLease));

        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Ending lease rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        Lease lease;
        try
        {
            var existingLease = await dbContext.Lease
                .Include(l => l.Room)
                .FirstOrDefaultAsync(l => l.Id == id && l.Room.UserId == userId, cancellationToken);

            if (existingLease is null)
            {
                return Results.NotFound(new LeaseResponseDto(false, "Lease not found."));
            }
            lease = existingLease;

            if (!lease.IsActive)
                return Results.Conflict(new LeaseResponseDto(false, "Only an active lease can be ended."));

            if (request.EndDate < lease.StartDate)
                return Results.BadRequest(new LeaseResponseDto(false, "The end date cannot be before the lease start date."));

            lease.EndDate = request.EndDate;
            lease.IsActive = false;

            if (lease.Room.Status != RoomStatus.Maintenance)
            {
                lease.Room.Status = RoomStatus.Available;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Ending lease failed: an expected lease or room row was not updated.");
            return Results.Conflict(new LeaseResponseDto(false, "The lease or room is no longer available. Reload and try again."));
        }
        catch (Exception ex) when (ex is PostgresException
        {
            SqlState: PostgresErrorCodes.SerializationFailure
        } || ex is DbUpdateException
        {
            InnerException: PostgresException { SqlState: PostgresErrorCodes.SerializationFailure }
        })
        {
            logger.LogWarning("Ending lease conflicted with another database transaction.");
            return Results.Conflict(new LeaseResponseDto(false, "Availability changed while saving. Reload and try again."));
        }
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Lease {LeaseId} ended for room {RoomId} on {EndDate}.",
                lease.Id, lease.RoomId, lease.EndDate);
        }

        return Results.Ok(new LeaseDetailsDto(lease.Id, lease.RoomId, lease.TenantId,
            lease.MonthlyRent, lease.StartDate, lease.EndDate, lease.IsActive, lease.CreatedAt));
    }
}

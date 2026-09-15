using System.Data;
using System.Security.Claims;
using HouseRentMgmt.Api.Features.Rooms.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseRentMgmt.Api.Features.Rooms.RoomHandler;

public static class EditRoom
{
    public static void MapEditRoom(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:guid}", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        Guid id,
        EditRoomRequestDto editRoomRequestDto,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(EditRoom));
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Room update rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        // Read the lease state and save the room in the same isolation level
        // used by lease creation/ending, so competing changes cannot bypass the guard.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        Room room;
        try
        {
            var existingRoom = await dbContext.Room.FirstOrDefaultAsync(
                r => r.Id == id && r.UserId == userId, cancellationToken);

            if (existingRoom is null)
            {
                logger.LogWarning("Room update rejected: the room was not found for the authenticated user.");
                return Results.NotFound(new RoomResponseDto(false, "Room not found.", null));
            }
            room = existingRoom;

            var hasActiveLease = await dbContext.Lease.AnyAsync(
                lease => lease.RoomId == room.Id && lease.IsActive, cancellationToken);

            if (hasActiveLease && editRoomRequestDto.Status != RoomStatus.Occupied)
            {
                return Results.Conflict(new RoomResponseDto(false,
                    "End the active lease before changing the room status.", null));
            }

            if (!hasActiveLease && editRoomRequestDto.Status == RoomStatus.Occupied)
            {
                return Results.Conflict(new RoomResponseDto(false,
                    "Create a lease to mark this room occupied.", null));
            }

            room.RoomName = editRoomRequestDto.RoomName;
            room.FloorId = editRoomRequestDto.FloorId;
            room.BaseRentAmount = editRoomRequestDto.BaseRentAmount;
            room.Status = editRoomRequestDto.Status;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Room update failed: the expected room row was not updated; it may have been deleted before saving.");
            return Results.Conflict(new RoomResponseDto(false, "The room could not be updated. It may have been deleted. Reload and try again.", null));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_Room_UserId_RoomName"
        })
        {
            logger.LogWarning("Room update rejected: the room name already exists for this user.");
            return Results.Conflict(new RoomResponseDto(false, "You already have a room with this name.", null));
        }

        catch (Exception ex) when (ex is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure }
            || ex is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.SerializationFailure } })
        {
            logger.LogWarning("Room update conflicted with another database transaction.");
            return Results.Conflict(new RoomResponseDto(false,
                "The room or lease changed while saving. Reload and try again.", null));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Room {RoomId} edited by user {UserId} with status {RoomStatus}.",
                id, userId, editRoomRequestDto.Status);
        }

        var roomDetails = new RoomDetails(
            room.Id,
            room.FloorId,
            room.RoomName,
            room.BaseRentAmount,
            room.Status,
            room.CreatedAt);
        
        var response = new RoomResponseDto(true, "Room edited successfully.", roomDetails);

        return Results.Ok(response);
    }
}

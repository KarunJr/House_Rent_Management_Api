using System.Security.Claims;
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
        ILoggerFactory loggerFactory
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(EditRoom));
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Room update rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        var room = await dbContext.Room.FirstOrDefaultAsync(
            r => r.Id == id && r.UserId == userId);

        if (room is null)
        {
            logger.LogWarning("Room update rejected: the room was not found for the authenticated user.");
            return Results.NotFound(new RoomResponseDto(false, "Room not found.", null));
        }

        room.RoomName = editRoomRequestDto.RoomName;
        room.FloorId = editRoomRequestDto.FloorId;
        room.BaseRentAmount = editRoomRequestDto.BaseRentAmount;
        room.Status = editRoomRequestDto.Status;

        try
        {
            await dbContext.SaveChangesAsync();
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

using System.Security.Claims;
using HouseRentMgmt.Api.Features.Rooms.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseRentMgmt.Api.Features.Rooms.RoomHandler;

public static class AddRoom
{
    public static void MapAddRoom(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        RoomRequestDto roomRequestDto,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(AddRoom));
        if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            logger.LogWarning("Room creation rejected: the authenticated user ID claim is missing or invalid.");
            return Results.Unauthorized();
        }

        var newRoom = new Room
        {
            RoomName = roomRequestDto.RoomName,
            FloorId = roomRequestDto.FloorId,
            UserId = userId,
            BaseRentAmount = roomRequestDto.BaseRentAmount,
            Status = roomRequestDto.Status
        };

        dbContext.Room.Add(newRoom);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_Room_UserId_RoomName"
        })
        {
            logger.LogWarning("Room creation rejected: the room name already exists for this user.");
            return Results.Conflict(new RoomResponseDto(false, "You already have a room with this name."));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: "FK_Room_AspNetUsers_UserId"
        })
        {
            logger.LogWarning("Room creation rejected: the authenticated user's account no longer exists.");
            return Results.Unauthorized();
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Room {RoomId} created for user {UserId} with status {RoomStatus}.",
                newRoom.Id, newRoom.UserId, newRoom.Status);
        }

        var response = new RoomDetailsDto(
            newRoom.Id,
            newRoom.FloorId,
            newRoom.RoomName,
            newRoom.BaseRentAmount,
            newRoom.Status,
            newRoom.CreatedAt);

        return Results.Created($"/webservice/v1/api/room/{newRoom.Id}", response);
    }
}

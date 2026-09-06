using System.Security.Claims;
using HouseRentMgmt.Api.Features.Rooms.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HouseRentMgmt.Api.Features.Rooms.RoomHandler;

public static class AddRoom
{
    public static void MapAddRoom(this IEndpointRouteBuilder app)
    {
        app.MapPost("/add", HandleAsync).RequireAuthorization();
    }

    public static async Task<IResult> HandleAsync
    (
        RoomRequestDto roomRequestDto,
        ClaimsPrincipal user,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(AddRoom));
        // if (!Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        // {
        //     logger.LogWarning("Room creation rejected: the authenticated user ID claim is missing or invalid.");
        //     return Results.Unauthorized();
        // }

        var newRoom = new Room
        {
            RoomName= roomRequestDto.RoomName,
            FloorId= roomRequestDto.FloorId,
            UserId= Guid.Parse("01a07621-957a-77cd-b9c3-52c93df2598b"),
            BaseRentAmount = roomRequestDto.BaseRentAmount,
            Status = roomRequestDto.Status
        };
        dbContext.Room.Add(newRoom);
        await dbContext.SaveChangesAsync();
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

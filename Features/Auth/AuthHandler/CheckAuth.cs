using System.Security.Claims;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HouseRentMgmt.Api.Features.Auth.AuthHandler;

public static class CheckAuth
{
    public static void MapCheckAuth(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", HandleAsync).RequireAuthorization();
    }

    public static async Task<IResult> HandleAsync
    (
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal claims
    )
    {
        if (!Guid.TryParse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Results.Unauthorized();
        }
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null || user.Email is null || user.UserName is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new UserResponseDto(
            user.Id,
            user.Name,
            user.UserName,
            user.Email
        ));

    }
}

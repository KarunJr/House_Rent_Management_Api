using HouseRentMgmt.Api.Infrastructure.Data;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
namespace HouseRentMgmt.Api.Features.Auth.AuthHandler;

public static class VerifyEmail
{
    public static void MapVerifyEmail(this IEndpointRouteBuilder app)
    {
        app.MapPost("/verify-email", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        VerifyEmailRequestDto verifyDto,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext
    )
    {
        return Results.Ok();
    }
}

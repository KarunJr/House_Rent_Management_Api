using HouseRentMgmt.Api.Data;
using HouseRentMgmt.Api.DTO;
using HouseRentMgmt.Api.Models;
using HouseRentMgmt.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HouseRentMgmt.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/webservice/v1/api/auth");

        auth.MapPost("/register", async (ITokenService tokenService, CreateUserDto userDto, UserManager<ApplicationUser> userManager) =>
        {
            var userExists = await userManager.FindByEmailAsync(userDto.Email);

            if (userExists != null)
            {
                return Results.BadRequest(new { message = "Email is already registered." });
            }

            var newUser = new ApplicationUser
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                Name = userDto.Name,
                PhoneNumber = userDto.Phone
            };

            var result = await userManager.CreateAsync(newUser, userDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return Results.BadRequest(new { errors });
            }

            return Results.Ok(new {message = "User reigstered successfully."});
        });
    }
}

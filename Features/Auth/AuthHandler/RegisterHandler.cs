using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HouseRentMgmt.Api.Features.Auth.AuthHandler;

public static class RegisterHandler
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/register", HandleAsync);
    }

    public static async Task<IResult> HandleAsync(ITokenService tokenService, CreateUserDto userDto, UserManager<ApplicationUser> userManager, HttpContext httpContext, ICookieService cookie)
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
        var tokenValues = new TokenUserDto(newUser.Id, newUser.Name, newUser.UserName);
        var token = tokenService.GenerateToken(tokenValues);
        

        cookie.SetCookie(context: httpContext, "jwt_accesstoken", token);

        Console.WriteLine($"Token is created on {DateTime.Now} and Token is {token}");
        return Results.Ok(new { message = "User reigstered successfully." });
    }
};

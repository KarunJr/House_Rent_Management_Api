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

    public static async Task<IResult> HandleAsync
    (
        CreateUserDto userDto,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ICookieService cookie,
        HttpContext httpContext
    )
    {
        var emailExist = await userManager.FindByEmailAsync(userDto.Email);
        if (emailExist != null)
        {
            return Results.BadRequest(new { message = "Email is already registered." });
        }

        var userExist = await userManager.FindByNameAsync(userDto.Username);
        if (userExist != null)
        {
            return Results.BadRequest(new { message = "Username is already taken." });
        }

        var newUser = new ApplicationUser
        {
            UserName = userDto.Username,
            Email = userDto.Email,
            Name = userDto.Name,
            PhoneNumber = userDto.Phone
        };

        var result = await userManager.CreateAsync(newUser, userDto.Password);
        // This is for Concurrency.
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code == "DuplicateEmail"))
            {
                return Results.BadRequest(new { message = "Email is already registered." });
            }
            if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                return Results.BadRequest(new { message = "Username is already taken." });
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return Results.BadRequest(new { errors });
        }
        var tokenValues = new TokenUserDto(newUser.Id, newUser.Name, newUser.UserName);
        var token = tokenService.GenerateToken(tokenValues);


        cookie.SetCookie(context: httpContext, "jwt_accesstoken", token);

        Console.WriteLine($"Token is created on {DateTime.Now} and Token is {token}");
        return Results.Ok(new { message = "User registered successfully." });
    }
};

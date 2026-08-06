using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
using HouseRentMgmt.Api.Features.Auth.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Features.Auth.AuthHandler;

public static class LoginHandler
{
    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        LoginRequestDto loginRequestDto,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IEmailService emailService,
        IOtpService otpService,
        ITokenService tokenService
    )
    {
        var invalidCredentialsResponse = Results.Ok(new LoginResponseDto(
            Success: false,
            Message: "Invalid username/email or password",
            User: null,
            EmailVerified: null,
            Token: null
        ));
        try
        {
            var user = await userManager.FindByEmailAsync(loginRequestDto.UsernameOrEmail) ?? await userManager.FindByNameAsync(loginRequestDto.UsernameOrEmail);

            if (user == null || user.Email == null || user.UserName == null)
            {
                return invalidCredentialsResponse;
            }

            var verifyUser = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (!verifyUser)
            {
                return invalidCredentialsResponse;
            }

            var userResponse = new UserResponseDto(
                Id: user.Id,
                Name: user.Name,
                Username: user.UserName,
                Email: user.Email
            );

            if (!user.EmailConfirmed)
            {
                var existingOtps = await dbContext.EmailVerificationCode.Where(x => x.UserId == user.Id).ToListAsync();

                if (existingOtps.Count != 0)
                {
                    dbContext.EmailVerificationCode.RemoveRange(existingOtps);
                    await dbContext.SaveChangesAsync();
                }
                var otp = otpService.GenerateOtp();
                try
                {
                    var emailVerify = new EmailVerificationCode
                    {
                        UserId = user.Id,
                        OtpCode = otp,
                        AttemptCount = 0
                    };
                    await dbContext.EmailVerificationCode.AddAsync(emailVerify);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OTP email failed to send for {user.Email}. Error: {ex.Message}");
                    return Results.InternalServerError(new ApiErrorResponse(Message: "An internal error occurred during registration."));
                }

                try
                {
                    await emailService.SendEmailAsync(user.Email, user.Name, otp);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OTP email failed to send for {user.Email}. Error: {ex.Message}");

                    return Results.Ok(new LoginResponseDto(
                        Success: false,
                        Message: "Please verify your email. We couldn't send the code right now, please request a new one.",
                        User: userResponse,
                        EmailVerified: false,
                        Token: null
                    ));
                }
                return Results.Ok(new LoginResponseDto(
                    Success: false,
                    Message: "Please verify your email before logging in.",
                    User: userResponse,
                    EmailVerified: false,
                    Token: null
                ));
            }

            var token = tokenService.GenerateToken(new TokenUserDto(
                Id: user.Id,
                Name: user.Name,
                Username: user.UserName
            ));
            return Results.Ok(new LoginResponseDto(
                Success: true,
                Message: "Logged in successfully",
                User: userResponse,
                EmailVerified: true,
                Token: token
            ));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return Results.InternalServerError(new ApiErrorResponse("Something went wrong"));
        }

    }
}

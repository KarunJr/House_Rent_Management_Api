using System.Net;
using HouseRentMgmt.Api.Features.Auth.AuthServices;
using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
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
            EmailSent: null,
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
                var otp = otpService.GenerateOtp();
                var now = DateTime.UtcNow;
                var expiresAt = now.AddMinutes(10);
                try
                {
                    await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO ""EmailVerificationCode"" (""UserId"", ""OtpCode"", ""CreatedAt"", ""ExpiresAt"", ""AttemptCount"")
                        VALUES ({user.Id}, {otp}, {now}, {expiresAt}, 0)
                        ON CONFLICT (""UserId"")
                        DO UPDATE SET
                            ""OtpCode"" = {otp}, 
                            ""CreatedAt"" = {now},
                            ""ExpiresAt"" = {expiresAt},
                            ""AttemptCount"" = 0;
                    ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save verification OTP to database for {user.Email}. Error: {ex.Message}");
                    return Results.InternalServerError(new ApiErrorResponse(Message: "An internal error occurred during login."));
                }

                try
                {
                    await emailService.SendEmailAsync(user.Email, user.Name, otp);
                }
                catch (BrevoEmailException brevoEx)
                {
                    string userFriendlyMsg = brevoEx.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => "Email system misconfigured. Please contact support.",
                        HttpStatusCode.PaymentRequired => "Email delivery is temporarily paused.",
                        HttpStatusCode.BadRequest => "Invalid request details provided.",
                        _ => "We couldn't send the code right now, please request a new one."
                    };

                    return Results.Ok(new LoginResponseDto(
                        Success: false,
                        Message: userFriendlyMsg,
                        User: userResponse,
                        EmailVerified: false,
                        EmailSent: false,
                        Token: null
                    ));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OTP email failed to send for {user.Email}. Error: {ex.Message}");

                    return Results.Ok(new LoginResponseDto(
                        Success: false,
                        Message: "Please verify your email. We couldn't send the code right now, please request a new one.",
                        User: userResponse,
                        EmailVerified: false,
                        EmailSent: false,
                        Token: null
                    ));
                }

                return Results.Ok(new LoginResponseDto(
                    Success: false,
                    Message: "Your account is not verified yet. We have sent a verification email to your inbox—please check it to activate your account.",
                    User: userResponse,
                    EmailVerified: false,
                    EmailSent: true,
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
                EmailSent: null,
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

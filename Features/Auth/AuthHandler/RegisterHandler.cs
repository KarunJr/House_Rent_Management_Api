using System.Net;
using HouseRentMgmt.Api.Features.Auth.AuthServices;
using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
using HouseRentMgmt.Api.Features.Auth.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
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
        UserRegistrationRequestDto userDto,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IOtpService otpService,
        IEmailService emailService,
        ILoggerFactory loggerFactory
    )
    {
        var logger = loggerFactory.CreateLogger(nameof(RegisterHandler));
        if (await userManager.FindByEmailAsync(userDto.Email) != null)
        {
            return Results.BadRequest(new ApiErrorResponse(Message: "Email is already registered."));
        }

        if (await userManager.FindByNameAsync(userDto.Username) != null)
        {
            return Results.BadRequest(new ApiErrorResponse(Message: "Username is already taken."));
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
                return Results.BadRequest(new ApiErrorResponse(Message: "Email is already registered."));
            }
            if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                return Results.BadRequest(new ApiErrorResponse(Message: "Username is already taken."));
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return Results.BadRequest(new ApiErrorResponse(Message: "Registration failed.", Errors: errors));
        }

        Guid userId = newUser.Id;
        var otp = otpService.GenerateOtp();
        try
        {
            var emailVerify = new EmailVerificationCode
            {
                UserId = userId,
                OtpCode = otp,
                AttemptCount = 0
            };
            await dbContext.EmailVerificationCode.AddAsync(emailVerify);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create a verification code for user {UserId}.", userId);
            try
            {
                var deleteResult = await userManager.DeleteAsync(newUser);
                if (!deleteResult.Succeeded)
                {
                    logger.LogCritical("Failed to roll back user registration for user {UserId}.", userId);
                }
            }
            catch (Exception deleteEx)
            {
                logger.LogCritical(deleteEx, "Failed to roll back user registration for user {UserId}.", userId);
            }
            return Results.InternalServerError(new ApiErrorResponse(Message: "An internal error occurred during registration."));
        }

        var createdUser = new UserResponseDto(
            Id: newUser.Id,
            Name: newUser.Name,
            Username: newUser.UserName,
            Email: newUser.Email
        );

        try
        {
            await emailService.SendEmailAsync(newUser.Email, newUser.Name, otp);
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

            return Results.Ok(new UserRegistrationResponseDto(
                Message: userFriendlyMsg,
                EmailSent: false,
                CreatedUser: createdUser
            ));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Registration completed, but the verification email could not be sent for user {UserId}.", userId);

            return Results.Ok(new UserRegistrationResponseDto(
                Message: "User registered successfully, but we couldn't send the verification email right now. Please log in and request a new code.",
                EmailSent: false,
                CreatedUser: createdUser
            ));
        }

        return Results.Ok(new UserRegistrationResponseDto
        (
            Message: "User registered successfully. Please check your email for the verification code.",
            EmailSent: true,
            CreatedUser: createdUser
        ));
    }
};

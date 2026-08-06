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
        IEmailService emailService
    )
    {
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
            Console.WriteLine($"Registration email failed for {newUser.Email}. Error: {ex.Message}");
            try
            {
                await userManager.DeleteAsync(newUser);
            }
            catch (Exception deleteEx)
            {
                Console.WriteLine($"Critical: Failed to roll back/delete user account {userId}. Error: {deleteEx.Message}");
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
        catch (Exception ex)
        {
            Console.WriteLine($"Registration email failed for {newUser.Email}. Error: {ex.Message}");

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

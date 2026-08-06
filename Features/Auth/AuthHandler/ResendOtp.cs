using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
using HouseRentMgmt.Api.Features.Auth.Entities;
using HouseRentMgmt.Api.Infrastructure.Data;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseRentMgmt.Api.Features.Auth.AuthHandler;

public static class ResendOtp
{
    public static void MapResendEmail(this IEndpointRouteBuilder app)
    {
        app.MapPost("/resend-otp", HandleAsync);
    }

    public static async Task<IResult> HandleAsync
    (
        ResendOtpRequestDto resendDto,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IOtpService otpService,
        IEmailService emailService
    )
    {
        var user = await userManager.FindByEmailAsync(resendDto.Email);

        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return Results.BadRequest(new ApiErrorResponse(Message: "User not found"));
        }
        if (user.EmailConfirmed)
        {
            return Results.BadRequest(new ApiErrorResponse(Message: "Email is already verified!"));
        }

        var existingOtps = await dbContext.EmailVerificationCode.Where(x => x.UserId == user.Id).ToListAsync();

        if (existingOtps.Count != 0)
        {
            dbContext.EmailVerificationCode.RemoveRange(existingOtps);
        }

        var newOtp = otpService.GenerateOtp();
        
        try
        {
            var emailVerify = new EmailVerificationCode
            {
                UserId = user.Id,
                OtpCode = newOtp,
                AttemptCount = 0
            };

            await dbContext.EmailVerificationCode.AddAsync(emailVerify);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Resend OTP database save failed for {user.Email}. Error: {ex.Message}");
            return Results.InternalServerError(new ApiErrorResponse(Message: "An internal error occurred while generating the verification code."));
        }
        try
        {
            await emailService.SendEmailAsync(user.Email, user.Name, newOtp);
            return Results.Ok(new ResendOtpResponseDto(
                Message: "Verification code resent successfully. Please check your email.",
                EmailSent: true
            ));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Resend email failed for {user.Email}. Error: {ex.Message}");

            return Results.InternalServerError(new ResendOtpResponseDto(
                Message: "We couldn't send the verification email right now. Please log in and request a new code.",
                EmailSent: false
            ));
        }
    }
}

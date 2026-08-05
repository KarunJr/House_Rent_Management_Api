using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;
using HouseRentMgmt.Api.Infrastructure.Data;
using HouseRentMgmt.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        ApplicationDbContext dbContext,
        IOtpService otpService
    )
    {
        try
        {
            var user = await userManager.FindByEmailAsync(verifyDto.Email);
            if (user == null)
            {
                return Results.BadRequest(new ApiErrorResponse(Message: "User not found"));
            }
            if (user.EmailConfirmed)
            {
                return Results.BadRequest(new ApiErrorResponse(Message: "Email is already verified!"));
            }

            var otp = await dbContext.EmailVerificationCode.FirstOrDefaultAsync(x => x.UserId == user.Id);


            if (otp == null)
            {
                return Results.BadRequest(new ApiErrorResponse(Message: "No verification code found. Please request a new OTP."));
            }

            var otpResult = otpService.VerifyOtp(verifyDto.Otp, otp);

            switch (otpResult)
            {
                case OtpVerificationResult.Success:
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                    dbContext.EmailVerificationCode.Remove(otp);
                    await dbContext.SaveChangesAsync();

                    return Results.Ok(new VerifyEmailResponseDto(Success: true, Message: "Email verified successfully"));

                case OtpVerificationResult.InvalidCode:
                    await dbContext.SaveChangesAsync();
                    return Results.BadRequest(new ApiErrorResponse(Message: "Invalid OTP"));

                case OtpVerificationResult.Expired:
                    return Results.BadRequest(new ApiErrorResponse(Message: "OTP expired"));

                case OtpVerificationResult.TooManyAttempts:
                    return Results.BadRequest(new ApiErrorResponse(Message: "Too many attempts. Please request a new OTP"));

                case OtpVerificationResult.AlreadyUsed:
                    return Results.BadRequest(new ApiErrorResponse(Message: "OTP already used"));

                default:
                    return Results.BadRequest(new ApiErrorResponse(Message: "OTP verification failed"));
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Exception occurred while verifying email for '{verifyDto.Email}': {ex.Message} | StackTrace: {ex.StackTrace}");
            return Results.InternalServerError(new ApiErrorResponse(Message: "An internal server error occurred while verifying the OTP. Please try again later."));
        }
    }
}

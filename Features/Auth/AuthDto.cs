using System.ComponentModel.DataAnnotations;
using HouseRentMgmt.Api.Infrastructure.Validation;

namespace HouseRentMgmt.Api.Features.Auth;

public record TokenUserDto
(
    Guid Id,
    string Name,
    string Username
);

public record UserRegistrationRequestDto(
    string Name,
    string Username,
    string Email,
    string Password,
    string Phone)
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; init; } = Name?.Trim()!;
    [Required(ErrorMessage = "Username is required")]
    [RegularExpression(RequestPatterns.Username, ErrorMessage = "Username contains invalid characters")]
    public string Username { get; init; } = Username?.Trim()!;
    [Required(ErrorMessage = "Invalid email address")]
    [RegularExpression(RequestPatterns.Email, ErrorMessage = "Invalid email address")]
    public string Email { get; init; } = Email?.Trim().ToLowerInvariant()!;
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [RegistrationPassword]
    public string Password { get; init; } = Password;
    [Required(ErrorMessage = "Invalid Nepali mobile number format")]
    [RegularExpression(RequestPatterns.Phone, ErrorMessage = "Invalid Nepali mobile number format")]
    public string Phone { get; init; } = Phone?.Trim()!;
}


public record UserResponseDto
(
    Guid Id,
    string Name,
    string Username,
    string Email
);

public record UserRegistrationResponseDto
(
    string Message,
    bool EmailSent,
    UserResponseDto CreatedUser
);

public record VerifyEmailRequestDto(
    string Email,
    string Otp)
{
    [Required(ErrorMessage = "Invalid email address")]
    [RegularExpression(RequestPatterns.Email, ErrorMessage = "Invalid email address")]
    public string Email { get; init; } = Email?.Trim().ToLowerInvariant()!;
    [Required(ErrorMessage = "Verification code is required")]
    [RegularExpression(@"[0-9]{6}", ErrorMessage = "Verification code must contain 6 digits")]
    public string Otp { get; init; } = Otp;
}

public record VerifyEmailResponseDto
(
    bool Success,
    string Message,
    UserResponseDto CreatedUser,
    string Token
);
public record ResendOtpRequestDto(
    string Email)
{
    [Required(ErrorMessage = "Invalid email address")]
    [RegularExpression(RequestPatterns.Email, ErrorMessage = "Invalid email address")]
    public string Email { get; init; } = Email?.Trim().ToLowerInvariant()!;
}

public record ResendOtpResponseDto
(
    string Message,
    bool EmailSent
);

public record LoginRequestDto(
    string UsernameOrEmail,
    string Password)
{
    [Required(ErrorMessage = "Username or Email is required")]
    public string UsernameOrEmail { get; init; } = UsernameOrEmail?.Trim()!;
    [Required(AllowEmptyStrings = true, ErrorMessage = "Password is required")]
    [MinLength(1, ErrorMessage = "Password is required")]
    public string Password { get; init; } = Password;
}

public record LoginResponseDto
(
    bool Success,
    string Message,
    UserResponseDto? User,
    bool? EmailVerified,
    bool? EmailSent,
    string? Token
);

// This response must be put in global area not here: Do later
public record ApiErrorResponse(
    string Message,
    List<string>? Errors = null
);
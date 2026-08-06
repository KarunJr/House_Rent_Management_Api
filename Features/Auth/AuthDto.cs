namespace HouseRentMgmt.Api.Features.Auth;

public record TokenUserDto
(
    Guid Id,
    string Name,
    string Username
);

public record UserRegistrationRequestDto
(
    string Name,
    string Username,
    string Email,
    string Password,
    string Phone
);

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

public record VerifyEmailRequestDto
(
    string Email,
    string Otp
);
public record VerifyEmailResponseDto
(
    bool Success,
    string Message,
    UserResponseDto CreatedUser,
    string Token
);
public record ResendOtpRequestDto
(
    string Email
);
public record ResendOtpResponseDto
(
    string Message,
    bool EmailSent
);

public record LoginRequestDto
(
    string UsernameOrEmail,
    string Password
);
public record LoginResponseDto
(
    bool Success,
    string Message,
    UserResponseDto? User,
    bool? EmailVerified,
    string? Token
);

// This response must be put in global area not here: Do later
public record ApiErrorResponse(
    string Message,
    List<string>? Errors = null
);
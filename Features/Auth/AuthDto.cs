namespace HouseRentMgmt.Api.Features.Auth;

public record TokenUserDto
(
    Guid Id,
    string Name,
    string Username
);

public record CreateUserDto
(
    string Name,
    string Username,
    string Email,
    string Password,
    string Phone
);

public record ResponseUserDto
(
    Guid Id,
    string Name, 
    string Username,
    string Email
);
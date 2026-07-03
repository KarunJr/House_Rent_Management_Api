namespace HouseRentMgmt.Api.DTO;


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
namespace HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

public interface IEmailService
{
    public Task SendEmailAsync(string to, string name, string token);

}

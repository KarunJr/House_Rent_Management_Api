using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices;

public class EmailService(HttpClient httpClient, IConfiguration config) : IEmailService
{
    public async Task SendEmailAsync(string email, string name, string token)
    {
            var apiUrl = config["Email:BrevoAPIUrl"]
                ?? throw new InvalidOperationException("Brevo API URL is missing.");

            var apiKey = config["Email:BrevoAPIKey"]
                ?? throw new InvalidOperationException("Brevo API Key is missing.");

            var emailPayload = new
            {
                templateId = config["Email:BrevoTemplateId"],
                to = new[]
              {
              new {name, email},
          },
                @params = new
                {
                    name,
                    code = token
                }
            };
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Add("api-key", apiKey);
            request.Headers.Add("Accept", "application/json");

            request.Content = JsonContent.Create(emailPayload);

            var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Brevo API call failed. Status Code: {ex.StatusCode} for email: {email}. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
           Console.WriteLine($"An unexpected error occurred while sending email to {email}. Details: {ex}"); 
           throw;
        }
    }
}

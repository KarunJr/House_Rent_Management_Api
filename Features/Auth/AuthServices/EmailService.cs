using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices;

public class EmailService(HttpClient httpClient, IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendEmailAsync(string email, string name, string token)
    {
        var apiUrl = config["Email:BrevoAPIUrl"]
            ?? throw new InvalidOperationException("Brevo API URL is missing.");

        var apiKey = config["Email:BrevoAPIKey"]
            ?? throw new InvalidOperationException("Brevo API Key is missing.");

        var emailPayload = new
        {
            templateId = int.Parse(config["Email:BrevoTemplateId"] ?? throw new InvalidOperationException("Brevo Template Id is missing.")),
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
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new BrevoEmailException(statusCode: response.StatusCode, responseBody: responseBody);
            }
        }
        catch (BrevoEmailException ex)
        {
            logger.LogError(ex, "Email delivery provider rejected the request with status {StatusCode}.", (int)ex.StatusCode);
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Email delivery request failed.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email delivery failed.");
            throw;
        }
    }
}

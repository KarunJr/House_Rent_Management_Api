using System.Net;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices;

public class BrevoEmailException(HttpStatusCode statusCode, string responseBody) 
    : Exception($"Brevo email service failed with status code {statusCode}. Response: {responseBody}")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string ResponseBody { get; } = responseBody;
}
namespace HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

public interface ICookieService
{
    void SetCookie(HttpContext context, string cookieKey, string value);
    string GetCookie(string cookieKey);
}

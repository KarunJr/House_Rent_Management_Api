
using HouseRentMgmt.Api.Features.Auth.AuthServices.Interfaces;

namespace HouseRentMgmt.Api.Features.Auth.AuthServices;

public class CookieService : ICookieService
{
    public void SetCookie(HttpContext context, string cookieKey, string value)
    {
        CookieOptions options = new()
        {
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true
        };

        context.Response.Cookies.Append(cookieKey, value, options);    
    }
    public string GetCookie(string cookieKey)
    {
        return cookieKey;
    }

}

using System;

namespace WHMapper.Services.BrowserClientIdProvider.Extension;

public class BrowserClientIdCookieMiddleware
{
    private readonly RequestDelegate _next;

    public BrowserClientIdCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Cookies.ContainsKey("client_uid") && !context.Response.HasStarted)
        {
            var uid = Guid.NewGuid().ToString();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Secure = context.Request.IsHttps // Always set Secure based on HTTPS
            };

            context.Response.Cookies.Append("client_uid", uid, cookieOptions);
        }

        await _next(context);
    }
}

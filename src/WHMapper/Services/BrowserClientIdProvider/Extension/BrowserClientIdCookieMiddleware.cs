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
        if (!context.Request.Cookies.ContainsKey("client_uid"))
        {
            if (!context.Response.HasStarted)
            {
                var uid = Guid.NewGuid().ToString();

                context.Response.Cookies.Append("client_uid", uid, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = context.Request.IsHttps,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });
            }
        }

        await _next(context);
    }
}

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System;
using System.Security.Cryptography;

namespace WHMapper.Services.BrowserClientIdProvider;

public class BrowserClientIdProvider : IBrowserClientIdProvider
{
    private readonly ILogger<BrowserClientIdProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public BrowserClientIdProvider(ILogger<BrowserClientIdProvider> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string?> GetOrCreateClientIdAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogError("HttpContext is null. Cannot access cookies.");
            return Task.FromResult<string?>(null);
        }

        if (!context.Request.Cookies.ContainsKey("client_uid"))
        {
            var uid = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("client_uid", uid, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

            return Task.FromResult<string?>(uid);
        } 
        else
        {
            return Task.FromResult<string?>(context.Request.Cookies["client_uid"]);
        }
    }

}

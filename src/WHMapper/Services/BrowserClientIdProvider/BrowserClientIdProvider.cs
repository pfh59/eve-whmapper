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

    public Task<string?> GetClientIdAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogError("HttpContext is null. Cannot access cookies.");
            return Task.FromResult<string?>(null);
        }

        if (context.Request.Cookies.ContainsKey("client_uid"))
        {
            // If the cookie exists, return its value
            return Task.FromResult<string?>(context.Request.Cookies["client_uid"]);
        }
        return Task.FromResult<string?>(null);
    }

}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO.EveAPI.SSO;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveAuthStateProvider;

public class EveAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EveAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity.IsAuthenticated)
        {
            return anonymousState;
        }

        // Check token expiration
        var expClaim = user.FindFirst("exp")?.Value;

        if (expClaim != null && long.TryParse(expClaim, out var exp))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
            if (expirationTime<= DateTime.UtcNow)
            {
                return anonymousState;
            }
        }

        return new AuthenticationState(user);
    }
}

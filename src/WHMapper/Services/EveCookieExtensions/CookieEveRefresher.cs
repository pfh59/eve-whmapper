using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO.EveAPI.SSO;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveCookieExtensions;

internal sealed class  CookieEveRefresher(IOptionsMonitor<EVEOnlineAuthenticationOptions> eveOptionsMonitor)
{        
    public async Task ValidateOrRefreshCookieAsync(CookieValidatePrincipalContext validateContext, string scheme)
    {
        var accessTokenExpirationText = validateContext.Properties.GetTokenValue("expires_at");
        if (!DateTimeOffset.TryParse(accessTokenExpirationText, out var accessTokenExpiration))
        {
            return;
        }

        var options = eveOptionsMonitor.Get(scheme);
        var now = options.TimeProvider!.GetUtcNow();
        if (now < accessTokenExpiration.AddMinutes(-5))
        {
            return;
        }

        var refreshToken = Uri.EscapeDataString(validateContext.Properties.GetTokenValue("refresh_token"));
        var content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

        options.Backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.ClientId}:{options.ClientSecret}")));
        var response = await options.Backchannel.PostAsync(options.TokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            validateContext.RejectPrincipal();
            return;
        }

        var result = await response.Content.ReadAsStringAsync();
        EveToken newToken = JsonSerializer.Deserialize<EveToken>(result);

        if(newToken == null || string.IsNullOrEmpty(newToken.AccessToken))
        {
            validateContext.RejectPrincipal();
            return;
        }

        validateContext.ShouldRenew = true;
        var expiresAt = now + TimeSpan.FromSeconds(newToken.ExpiresIn);
        validateContext.Properties.StoreTokens([
            new() { Name = "access_token", Value = newToken.AccessToken },
            new() { Name = "refresh_token", Value = newToken.RefreshToken },
            new() { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) }
        ]);
    }
}

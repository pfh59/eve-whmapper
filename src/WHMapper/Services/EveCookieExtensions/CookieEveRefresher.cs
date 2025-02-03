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
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.SSO;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Services.EveCookieExtensions;

internal sealed class  CookieEveRefresher(IEveOnlineTokenProvider tokenProvider)
{        
    public async Task ValidateOrRefreshCookieAsync(CookieValidatePrincipalContext validateContext, string scheme)
    {
        if(validateContext == null)
        {
            return;
        }

        var accountId = validateContext?.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(string.IsNullOrEmpty(accountId))
        {
            validateContext?.RejectPrincipal();
            return;
        }

        var access_token = validateContext?.Properties.GetTokenValue("access_token");
        var refresh_token  = validateContext?.Properties.GetTokenValue("refresh_token");
        var accessTokenExpirationText = validateContext?.Properties.GetTokenValue("expires_at");
        if (!DateTimeOffset.TryParse(accessTokenExpirationText, out var accessTokenExpiration))
        {
            return;
        }


        if (await tokenProvider.GetToken(accountId) == null)
        {
            //add token to cache
            await tokenProvider.SaveToken(new UserToken
            {
                AccountId = accountId,
                AccessToken = access_token,
                RefreshToken = refresh_token,
                Expiry = accessTokenExpiration.UtcDateTime
            });
        }

        if(await tokenProvider.IsTokenExpire(accountId)==false)
        {
            return;
        }
  
        try
        {
            await tokenProvider.RefreshAccessToken(accountId);
        }
        catch (Exception)
        {
            await tokenProvider.ClearToken(accountId);
            validateContext?.RejectPrincipal();
            return;
        }
        var token = await tokenProvider.GetToken(accountId);
        if (token == null)
        {
            await tokenProvider.ClearToken(accountId);
            validateContext?.RejectPrincipal();
            return;
        }



        if (validateContext != null)
        {
            validateContext.ShouldRenew = true;
        }
        
        if (token.AccessToken == null || token.RefreshToken == null)
        {
            validateContext?.RejectPrincipal();
            return;
        }

        validateContext?.Properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = token.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = token.RefreshToken },
            new AuthenticationToken { Name = "expires_at", Value = token.Expiry.ToString("o", CultureInfo.InvariantCulture) }
        });

        await tokenProvider.SaveToken(token);
    }
}

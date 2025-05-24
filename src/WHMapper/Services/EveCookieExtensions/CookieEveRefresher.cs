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
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.BrowserClientIdProvider;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Services.EveCookieExtensions;

internal sealed class CookieEveRefresher(IBrowserClientIdProvider browserClientIdProvider, IEveOnlineTokenProvider tokenProvider,IEveMapperUserManagementService eveMapperUserManagement)
{        
    public async Task ValidateOrRefreshCookieAsync(CookieValidatePrincipalContext validateContext, string scheme)
    {
        var clientId = await browserClientIdProvider.GetClientIdAsync();
        
        if (string.IsNullOrEmpty(clientId))
        {
            validateContext?.RejectPrincipal();
            return;
        }
        var accountId = validateContext?.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(string.IsNullOrEmpty(accountId))
        {
            await eveMapperUserManagement.RemoveAuthenticateWHMapperUser(clientId);
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
            await eveMapperUserManagement.AddAuthenticateWHMapperUser(clientId, accountId, new UserToken
            {
                AccountId = accountId,
                AccessToken = access_token,
                RefreshToken = refresh_token,
                Expiry = accessTokenExpiration.UtcDateTime
            });
        }

        if(!await tokenProvider.IsTokenExpire(accountId))
        {
            return;
        }
  
        try
        {
            await tokenProvider.RefreshAccessToken(accountId);

            // Refresh all tokens associated with the clientId, except the current one.
            WHMapperUser[] accounts = await eveMapperUserManagement.GetAccountsAsync(clientId);
            accounts = accounts.Where(x => x.Id.ToString() != accountId).ToArray();
            foreach (var account in accounts)
            {
                await tokenProvider.RefreshAccessToken(account.Id.ToString());
            }
      
        }
        catch (Exception)
        {
            await eveMapperUserManagement.RemoveAuthenticateWHMapperUser(clientId);
            validateContext?.RejectPrincipal();
            return;
        }
        var token = await tokenProvider.GetToken(accountId);
        if (token == null)
        {
            await eveMapperUserManagement.RemoveAuthenticateWHMapperUser(clientId);
            validateContext?.RejectPrincipal();
            return;
        }


        if (validateContext != null)
        {
            validateContext.ShouldRenew = true;
        }
        
        if (token.AccessToken == null || token.RefreshToken == null)
        {
            await eveMapperUserManagement.RemoveAuthenticateWHMapperUser(clientId);
            validateContext?.RejectPrincipal();
            return;
        }


       // validateContex?.ReplacePrincipal(new ClaimsPrincipal(validationResult.ClaimsIdentity));
        validateContext?.Properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = token.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = token.RefreshToken },
            new AuthenticationToken { Name = "expires_at", Value = token.Expiry.ToString("o", CultureInfo.InvariantCulture) }
        });

         await eveMapperUserManagement.AddAuthenticateWHMapperUser(clientId, accountId, token);
    }
}

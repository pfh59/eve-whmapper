using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOAuthProvider;

namespace Microsoft.AspNetCore.Routing;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("");

        group.MapGet("/login", (string? returnUrl,string? clientId) => TypedResults.Challenge(GetAuthProperties(returnUrl,clientId )))
            .AllowAnonymous();

        group.MapPost("/logout", async ([FromForm] string? returnUrl,[FromForm]string? clientId, IEveMapperUserManagementService userManagementService, IEveMapperTracker eveMapperTracker) => 
        {
           
            if (!string.IsNullOrEmpty(clientId))
            {
                WHMapperUser[] accounts = await userManagementService.GetAccountsAsync(clientId);
                if (accounts != null && accounts.Length>0)
                {
                    foreach (var account in accounts)
                    {
                        await eveMapperTracker.StopTracking(account.Id);
                    }
                    
                    await Task.Delay(new TimeSpan(0, 0, 5));
                }
                 await userManagementService.RemoveAuthenticateWHMapperUser(clientId);
            }

            return TypedResults.SignOut(GetAuthProperties(returnUrl,null),[CookieAuthenticationDefaults.AuthenticationScheme]);
        });

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl, string? clientId)
    {
        const string PathBase = "/";

        returnUrl = ValidateAndNormalizeReturnUrl(returnUrl, PathBase);

        var authProperties = new AuthenticationProperties { RedirectUri = returnUrl };

        if (!string.IsNullOrEmpty(clientId))
        {
            authProperties.Items[".Token.clientId"] = clientId;
        }

        return authProperties;
    }

    private static string ValidateAndNormalizeReturnUrl(string? returnUrl, string pathBase)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return pathBase;
        }

        if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            return new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }

        return returnUrl[0] == '/' ? returnUrl : $"{pathBase}{returnUrl}";
    }
}
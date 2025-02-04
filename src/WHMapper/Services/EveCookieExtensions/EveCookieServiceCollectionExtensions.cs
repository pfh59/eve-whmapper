using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveCookieExtensions;

internal static partial class EveCookieServiceCollectionExtensions
{
    public static IServiceCollection ConfigureEveCookieRefresh(this IServiceCollection services, string eveCookieScheme, string eveScheme)
    {
        services.AddSingleton<CookieEveRefresher>();
        services.AddOptions<CookieAuthenticationOptions>(eveCookieScheme)
        .Configure<CookieEveRefresher>((eveCookieOptions, refresher) =>
        {
            eveCookieOptions.Events.OnValidatePrincipal = context => refresher.ValidateOrRefreshCookieAsync(context, eveScheme);
        });
        services.AddOptions<EVEOnlineAuthenticationOptions>(eveScheme).Configure(eveOptions =>
        {
            // Store the refresh_token.
            eveOptions.SaveTokens = true;
        });
        return services;
    }
}

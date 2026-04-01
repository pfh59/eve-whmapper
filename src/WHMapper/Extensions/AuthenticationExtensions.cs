using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveMapper.AuthorizationPolicies;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveJwkExtensions;
using WHMapper.Services.EveCookieExtensions;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddEveAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var evessoConf = configuration.GetSection("EveSSO");
        var evessoConfScopes = evessoConf.GetSection("DefaultScopes");

        services.AddAuthentication(EVEOnlineAuthenticationDefaults.AuthenticationScheme)
        .AddEVEOnline(EVEOnlineAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.ClientId = evessoConf["ClientId"] ?? throw new InvalidOperationException("ClientId is not configured in EveSSO settings.");
            options.ClientSecret = evessoConf["Secret"] ?? throw new InvalidOperationException("Secret is not configured in EveSSO settings.");
            options.CallbackPath = new PathString("/sso/callback");
            options.Scope.Clear();

            var scopes = evessoConfScopes.Get<string[]>();
            if (scopes != null)
            {
                foreach (string scope in scopes)
                    options.Scope.Add(scope);
            }

            options.SaveTokens = true;
            options.UsePkce = true;

            options.Events = new OAuthEvents
            {
                OnTicketReceived = async context =>
                {
                    var userManagerService = context.HttpContext.RequestServices.GetRequiredService<IEveMapperUserManagementService>();

                    var clientId = context.Properties?.GetTokenValue("clientId");
                    if (string.IsNullOrEmpty(clientId))
                    {
                        await Task.FromException(new Exception("ClientId is not set."));
                        return;
                    }

                    var accountId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(accountId))
                    {
                        await Task.FromException(new Exception("AccountId is not set."));
                        return;
                    }

                    var accessToken = context.Properties?.GetTokenValue("access_token");
                    var refreshToken = context.Properties?.GetTokenValue("refresh_token");
                    var expiresAt = context.Properties?.GetTokenValue("expires_at");

                    if (!DateTimeOffset.TryParse(expiresAt, out var accessTokenExpiration))
                    {
                        await Task.FromException(new Exception("Invalid access token expiration date."));
                        return;
                    }

                    var token = new UserToken
                    {
                        AccountId = accountId,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        Expiry = accessTokenExpiration.UtcDateTime
                    };

                    await userManagerService.AddAuthenticateWHMapperUser(clientId, accountId, token);
                }
            };
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = "WHMapper";
            options.SlidingExpiration = true;
            options.AccessDeniedPath = "/Forbidden/";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        })
        .AddEveOnlineJwtBearer();

        services.ConfigureEveCookieRefresh(CookieAuthenticationDefaults.AuthenticationScheme, EVEOnlineAuthenticationDefaults.AuthenticationScheme);

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.ConsentCookieValue = "true";
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("auth", opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Access", policy =>
                policy.Requirements.Add(new EveMapperAccessRequirement()));
            options.AddPolicy("Admin", policy =>
                policy.Requirements.Add(new EveMapperAdminRequirement()));
            options.AddPolicy("Map", policy =>
                policy.Requirements.Add(new EveMapperMapRequirement()));
        });

        services.AddScoped<IAuthorizationHandler, EveMapperAccessHandler>();
        services.AddScoped<IAuthorizationHandler, EveMapperAdminHandler>();
        services.AddScoped<IAuthorizationHandler, EveMapperMapHandler>();

        return services;
    }
}

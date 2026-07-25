using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace WHMapper.Services.EveJwkExtensions
{
    public static class EveOnlineJwtBearerExtensions
    {
        public static AuthenticationBuilder AddEveOnlineJwtBearer([NotNull] this AuthenticationBuilder builder, string applicationClientId)
        {
            if (string.IsNullOrWhiteSpace(applicationClientId))
                throw new InvalidOperationException("EveSSO ClientId is required to validate hub access tokens.");

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(EveOnlineJwkDefaults.SSOUrl);
            httpClient.DefaultRequestHeaders.Host = EveOnlineJwkDefaults.EVE_HOST;

            var response = httpClient.GetAsync(EveOnlineJwkDefaults.JWKEndpoint).Result.Content.ReadAsStringAsync().Result;
            var jwks = new JsonWebKeySet(response);
            var jwk = jwks.Keys.First();

            TokenValidationParameters tokenValidationParams = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = EveOnlineJwkDefaults.ValideAudience,
                ValidateIssuer = true,
                ValidIssuer = EveOnlineJwkDefaults.ValideIssuer,
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,

                IssuerSigningKey = jwk,
                ClockSkew = TimeSpan.FromSeconds(2), // CCP's servers seem slightly ahead (~1s)
            };

            return builder.AddJwtBearer(EveOnlineJwkDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParams;
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        // ValidAudience is "EVE Online", present in every CCP token, so it would
                        // also accept tokens minted for any other EVE application. "azp" is the
                        // client id the token was actually issued to.
                        var authorizedParty = context.Principal?.FindFirst("azp")?.Value;
                        if (!string.Equals(authorizedParty, applicationClientId, StringComparison.Ordinal))
                        {
                            context.Fail("Token was not issued to this application.");
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        return Task.CompletedTask;
                    }

                };
            });
        }
    }
}

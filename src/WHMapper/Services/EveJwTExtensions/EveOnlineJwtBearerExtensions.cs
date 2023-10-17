using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveJwkExtensions
{
    public static class EveOnlineJwtBearerExtensions
    {

        public static AuthenticationBuilder AddEveOnlineJwtBearer([NotNull] this AuthenticationBuilder builder)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(EveOnlineJwkDefaults.SSOUrl);
            httpClient.DefaultRequestHeaders.Host = EveOnlineJwkDefaults.ValideIssuer;

            var response = httpClient.GetAsync(EveOnlineJwkDefaults.JWKEndpoint).Result.Content.ReadAsStringAsync().Result;
            var jwks = new JsonWebKeySet(response);
            var jwk = jwks.Keys.First();


            TokenValidationParameters tokenValidationParams = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience= EveOnlineJwkDefaults.ValideAudience,
                ValidateIssuer = true,
                ValidIssuer = EveOnlineJwkDefaults.ValideIssuer,
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = jwk,
                ClockSkew = TimeSpan.FromSeconds(2), // CCP's servers seem slightly ahead (~1s)
            };


            return builder.AddJwtBearer(EveOnlineJwkDefaults.AuthenticationScheme,options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParams;
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
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


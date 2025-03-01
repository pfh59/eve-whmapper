﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using WHMapper.Models.DTO;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Services.EveOAuthProvider.Validators;

namespace WHMapper.Services.EveOAuthProvider
{
    public static class EVEOnlineAuthenticationExtensions
    {
        public static AuthenticationBuilder AddEVEOnline([NotNull] this AuthenticationBuilder builder)
        {
            return builder.AddEVEOnline(EVEOnlineAuthenticationDefaults.AuthenticationScheme, options => { });
        }

        public static AuthenticationBuilder AddEVEOnline(
            [NotNull] this AuthenticationBuilder builder,
            [NotNull] Action<EVEOnlineAuthenticationOptions> configuration)
        {
            return builder.AddEVEOnline(EVEOnlineAuthenticationDefaults.AuthenticationScheme, configuration);
        }

        public static AuthenticationBuilder AddEVEOnline(
            [NotNull] this AuthenticationBuilder builder,
            [NotNull] string scheme,
            [NotNull] Action<EVEOnlineAuthenticationOptions> configuration)
        {
            return builder.AddEVEOnline(scheme, EVEOnlineAuthenticationDefaults.AuthenticationScheme, configuration);
        }

        public static AuthenticationBuilder AddEVEOnline(
            [NotNull] this AuthenticationBuilder builder,
            [NotNull] string scheme,
            string caption,
            [NotNull] Action<EVEOnlineAuthenticationOptions> configuration)
        {
            builder.Services.TryAddSingleton<IPostConfigureOptions<EVEOnlineAuthenticationOptions>, EVEOnlinePostConfigureOptions>();
            builder.Services.TryAddSingleton<IEveOnlineTokenProvider, EveOnlineTokenProvider>();
            builder.Services.AddScoped<IEveOnlineAccessTokenValidator, EveOnlineAccessTokenValidator>();
            builder.Services.AddScoped<IClaimServices, ClaimServices>();
            builder.Services.AddScoped<IEveUserInfosServices, EveUserInfosServices>();

            return builder.AddOAuth<EVEOnlineAuthenticationOptions, EVEOnlineAuthenticationHandler>(scheme, caption, configuration);
        }
    }
}

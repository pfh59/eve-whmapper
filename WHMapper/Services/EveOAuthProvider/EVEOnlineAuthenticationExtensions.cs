using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

            return builder.AddOAuth<EVEOnlineAuthenticationOptions, EVEOnlineAuthenticationHandler>(scheme, caption, configuration);
        }
    }
}

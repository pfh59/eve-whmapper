using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WHMapper.Services.EveOAuthProvider
{
    public class EVEOnlinePostConfigureOptions : IPostConfigureOptions<EVEOnlineAuthenticationOptions>
    {
        /// <inheritdoc />
        public void PostConfigure(
            [NotNull] string name,
            [NotNull] EVEOnlineAuthenticationOptions options)
        {
            if (options.SecurityTokenHandler == null)
            {
                options.SecurityTokenHandler = new JsonWebTokenHandler();
            }
        }
    }
}

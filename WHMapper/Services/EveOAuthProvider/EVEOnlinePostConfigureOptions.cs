using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace WHMapper.Services.EveOAuthProvider
{
    public class EVEOnlinePostConfigureOptions : IPostConfigureOptions<EVEOnlineAuthenticationOptions>
    {
        /// <inheritdoc />
        public void PostConfigure(
            [NotNull] string name,
            [NotNull] EVEOnlineAuthenticationOptions options)
        {

        }
    }
}

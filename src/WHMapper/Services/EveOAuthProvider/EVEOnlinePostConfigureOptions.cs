using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WHMapper.Services.EveOAuthProvider
{
    public class EVEOnlinePostConfigureOptions : IPostConfigureOptions<EVEOnlineAuthenticationOptions>
    {
        public void PostConfigure(string? name, EVEOnlineAuthenticationOptions options)
        {

        }
    }
}

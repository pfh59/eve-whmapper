using Microsoft.Extensions.Options;

namespace WHMapper.Services.EveOAuthProvider
{
    public class EVEOnlinePostConfigureOptions : IPostConfigureOptions<EVEOnlineAuthenticationOptions>
    {
        public void PostConfigure(string? name, EVEOnlineAuthenticationOptions options)
        {

        }
    }
}

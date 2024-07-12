using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WHMapper.Shared.Services.EveOAuthProvider
{
    public class EVEOnlineAuthenticationOptions : OAuthOptions
    {
        /// <summary>
        /// Gets or sets the optional <see cref="JsonWebTokenHandler"/> to use.
        /// </summary>
        public JsonWebTokenHandler SecurityTokenHandler { get; set; } = default!;

        public EVEOnlineAuthenticationOptions()
        {
            ClaimsIssuer = EVEOnlineAuthenticationDefaults.Issuer;
            CallbackPath = EVEOnlineAuthenticationDefaults.CallbackPath;

            AuthorizationEndpoint = EVEOnlineAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = EVEOnlineAuthenticationDefaults.TokenEndpoint;
        }
    }
}

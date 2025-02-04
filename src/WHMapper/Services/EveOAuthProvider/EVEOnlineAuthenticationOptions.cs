using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WHMapper.Services.EveOAuthProvider
{
    public class EVEOnlineAuthenticationOptions : OAuthOptions
    {
        public string RevokeTokenEndpoint { get; private set; } = default!;
        public string JWKEndpoint { get; private set; } = default!;

        public EVEOnlineAuthenticationOptions()
        {
            ClaimsIssuer = EVEOnlineAuthenticationDefaults.Issuer;
            CallbackPath = EVEOnlineAuthenticationDefaults.CallbackPath;

            AuthorizationEndpoint = EVEOnlineAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = EVEOnlineAuthenticationDefaults.TokenEndpoint;
            RevokeTokenEndpoint = EVEOnlineAuthenticationDefaults.RevokeTokenEndpoint;
            JWKEndpoint = EVEOnlineAuthenticationDefaults.JWKEndpoint;            
        }
    }
}

using System;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace WHMapper.Services.EveOAuthProvider
{
    public class EVEOnlineAuthenticationOptions : OAuthOptions
    {
        public EVEOnlineAuthenticationOptions()
        {
            ClaimsIssuer = EVEOnlineAuthenticationDefaults.Issuer;
            CallbackPath = EVEOnlineAuthenticationDefaults.CallbackPath;

            AuthorizationEndpoint = EVEOnlineAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = EVEOnlineAuthenticationDefaults.TokenEndpoint;
        }


    }
}

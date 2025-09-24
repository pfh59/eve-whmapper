using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Encodings.Web;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Services.EveOAuthProvider.Validators;

namespace WHMapper.Services.EveOAuthProvider
{

    public partial class EVEOnlineAuthenticationHandler : OAuthHandler<EVEOnlineAuthenticationOptions>
    {
        private readonly IEveOnlineAccessTokenValidator _accessTokenValidator;
        private readonly IClaimServices _claimServices;
        public EVEOnlineAuthenticationHandler(
            IOptionsMonitor<EVEOnlineAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IEveOnlineAccessTokenValidator accessTokenValidator,
            IClaimServices claimServices
            )
            : base(options, logger, encoder)
        {
                _accessTokenValidator = accessTokenValidator;
                _claimServices = claimServices;
        }

        protected override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            try
            {
                return base.HandleRemoteAuthenticateAsync();
            }
            catch (SecurityTokenException ex)
            {
                return Task.FromResult(HandleRequestResult.Fail(ex));
            }
        }


        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            string? accessToken = tokens.AccessToken;
           
            if(!await _accessTokenValidator.ValidateAsync(tokens.AccessToken!))
            {
                throw new SecurityTokenException("Access token is invalid");  
            }

           var tokenClaims = await _claimServices.ExtractClaimsFromEVEToken(accessToken!);

            foreach (var claim in tokenClaims)
            {
                identity.AddClaim(claim);
            }
          
            return await base.CreateTicketAsync(identity, properties, tokens);
        }

    }
}

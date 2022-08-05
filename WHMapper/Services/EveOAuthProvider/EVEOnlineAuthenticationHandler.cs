using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WHMapper.Services.EveOAuthProvider
{

    public partial class EVEOnlineAuthenticationHandler : OAuthHandler<EVEOnlineAuthenticationOptions>
    {

        public EVEOnlineAuthenticationHandler(
            [NotNull] IOptionsMonitor<EVEOnlineAuthenticationOptions> options,
            [NotNull] ILoggerFactory logger,
            [NotNull] UrlEncoder encoder,
            [NotNull] ISystemClock clock)
            : base(options, logger, encoder, clock)
                {
                }

        protected override Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            string? accessToken = tokens.AccessToken;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("No access token was returned in the OAuth token.");
            }

            var tokenClaims = ExtractClaimsFromToken(accessToken);

            foreach (var claim in tokenClaims)
            {
                identity.AddClaim(claim);
            }

            return base.CreateTicketAsync(identity, properties, tokens);
        }

        protected virtual IEnumerable<Claim> ExtractClaimsFromToken([NotNull] string token)
        {
            try
            {
                JsonWebTokenHandler SecurityTokenHandle = new JsonWebTokenHandler();

                var securityToken = SecurityTokenHandle.ReadJsonWebToken(token);

                var nameClaim = ExtractClaim(securityToken, "name");
                var expClaim = ExtractClaim(securityToken, "exp");

                var claims = new List<Claim>(securityToken.Claims);

                claims.Add(new Claim(ClaimTypes.NameIdentifier, securityToken.Subject.Replace("CHARACTER:EVE:", string.Empty, StringComparison.OrdinalIgnoreCase), ClaimValueTypes.String, ClaimsIssuer));
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value, ClaimValueTypes.String, ClaimsIssuer));
                claims.Add(new Claim(ClaimTypes.Expiration, UnixTimeStampToDateTime(expClaim.Value), ClaimValueTypes.DateTime, ClaimsIssuer));

                var scopes = claims.Where(x => string.Equals(x.Type, "scp", StringComparison.OrdinalIgnoreCase)).ToList();

                if (scopes.Count > 0)
                {
                    claims.Add(new Claim(EVEOnlineAuthenticationDefaults.Scopes, string.Join(' ', scopes.Select(x => x.Value)), ClaimValueTypes.String, ClaimsIssuer));
                }

                return claims;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse JWT for claims from EVEOnline token.", ex);
            }
        }

        private static Claim ExtractClaim([NotNull] JsonWebToken token, [NotNull] string claim)
        {
            var extractedClaim = token.Claims.FirstOrDefault(x => string.Equals(x.Type, claim, StringComparison.OrdinalIgnoreCase));

            if (extractedClaim == null)
            {
                throw new InvalidOperationException($"The claim '{claim}' is missing from the EVEOnline JWT.");
            }

            return extractedClaim;
        }

        private static string UnixTimeStampToDateTime(string unixTimeStamp)
        {
            if (!long.TryParse(unixTimeStamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixTime))
            {
                throw new InvalidOperationException($"The value {unixTimeStamp} of the 'exp' claim is not a valid 64-bit integer.");
            }

            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            return offset.ToString("o", CultureInfo.InvariantCulture);
        }



    }
    
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace WHMapper.Services.EveOAuthProvider
{

    public partial class EVEOnlineAuthenticationHandler : OAuthHandler<EVEOnlineAuthenticationOptions>
    {

        public EVEOnlineAuthenticationHandler(
            [NotNull] IOptionsMonitor<EVEOnlineAuthenticationOptions> options,
            [NotNull] ILoggerFactory logger,
            [NotNull] UrlEncoder encoder)
            : base(options, logger, encoder)
        {

        }

        protected override Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            return base.ExchangeCodeAsync(context);
        }


        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
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


            var principal = new ClaimsPrincipal(identity);
            var context = new OAuthCreatingTicketContext(principal, properties, Context, Scheme, Options, Backchannel, tokens, tokens.Response!.RootElement);
            context.RunClaimActions();

            await Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
        }

        protected virtual IEnumerable<Claim> ExtractClaimsFromToken([NotNull] string token)
        {
            try
            {

                var securityToken = Options.SecurityTokenHandler.ReadJsonWebToken(token);

                var nameClaim = ExtractClaim(securityToken, "name");
                var expClaim = ExtractClaim(securityToken, "exp");

                var claims = new List<Claim>(securityToken.Claims);

                claims.Add(new Claim(ClaimTypes.NameIdentifier, securityToken.Subject.Replace("CHARACTER:EVE:", string.Empty, StringComparison.OrdinalIgnoreCase), ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value, ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
                claims.Add(new Claim(ClaimTypes.Expiration, UnixTimeStampToDateTime(expClaim.Value), ClaimValueTypes.DateTime, EVEOnlineAuthenticationDefaults.Issuer));

                var scopes = claims.Where(x => string.Equals(x.Type, "scp", StringComparison.OrdinalIgnoreCase)).ToList();

                if (scopes.Count > 0)
                {
                    claims.Add(new Claim(EVEOnlineAuthenticationDefaults.Scopes, string.Join(' ', scopes.Select(x => x.Value)), ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
                }

                return claims;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse JWT for claims from EVEOnline token.", ex);
            }
        }

        public static IEnumerable<Claim> ExtractClaimsFromEVEToken([NotNull] string token)
        {
            try
            {
                JsonWebTokenHandler securityTokenHandler = new JsonWebTokenHandler();
                var securityToken = securityTokenHandler.ReadJsonWebToken(token);

                var nameClaim = ExtractClaim(securityToken, "name");
                var expClaim = ExtractClaim(securityToken, "exp");

                var claims = new List<Claim>(securityToken.Claims);

                claims.Add(new Claim(ClaimTypes.NameIdentifier, securityToken.Subject.Replace("CHARACTER:EVE:", string.Empty, StringComparison.OrdinalIgnoreCase), ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value, ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
                claims.Add(new Claim(ClaimTypes.Expiration, UnixTimeStampToDateTime(expClaim.Value), ClaimValueTypes.DateTime, EVEOnlineAuthenticationDefaults.Issuer));

                var scopes = claims.Where(x => string.Equals(x.Type, "scp", StringComparison.OrdinalIgnoreCase)).ToList();

                if (scopes.Count > 0)
                {
                    claims.Add(new Claim(EVEOnlineAuthenticationDefaults.Scopes, string.Join(' ', scopes.Select(x => x.Value)), ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
                }

                return claims;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse JWT for claims from EVEOnline token.", ex);
            }
        }

        public static Claim ExtractClaim([NotNull] JsonWebToken token, [NotNull] string claim)
        {
            var extractedClaim = token.Claims.FirstOrDefault(x => string.Equals(x.Type, claim, StringComparison.OrdinalIgnoreCase));

            if (extractedClaim == null)
            {
                throw new InvalidOperationException($"The claim '{claim}' is missing from the EVEOnline JWT.");
            }

            return extractedClaim;
        }

        public static string UnixTimeStampToDateTime(string unixTimeStamp)
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

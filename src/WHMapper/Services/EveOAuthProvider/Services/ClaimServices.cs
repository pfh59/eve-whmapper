using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WHMapper.Services.EveOAuthProvider.Services;

public class ClaimServices : IClaimServices
{
    private readonly JwtSecurityTokenHandler _handler;

    public ClaimServices()
    {
        _handler = new JwtSecurityTokenHandler();
    }

    public async Task<IEnumerable<Claim>> ExtractClaimsFromEVEToken([NotNull] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentNullException(nameof(token));

        try
        {
            var securityToken = _handler.ReadToken(token) as JwtSecurityToken;

            var nameClaim = ExtractClaim(securityToken, "name");
            var expClaim = ExtractClaim(securityToken, "exp");



            return jsonToken.Claims;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse JWT for claims from EVEOnline token.", ex);
        }
    }

        public static Claim ExtractClaim([NotNull] JwtSecurityToken token, [NotNull] string claim)
        {
            var extractedClaim = token.Claims.FirstOrDefault(x => string.Equals(x.Type, claim, StringComparison.OrdinalIgnoreCase));

            if (extractedClaim == null)
            {
                throw new InvalidOperationException($"The claim '{claim}' is missing from the EVEOnline JWT.");
            }

            return extractedClaim;
        }



    /*
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

            return
}

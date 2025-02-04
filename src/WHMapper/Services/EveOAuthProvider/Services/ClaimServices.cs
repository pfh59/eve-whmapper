using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WHMapper.Services.EveOAuthProvider.Services;

public class ClaimServices : IClaimServices
{
    private readonly JsonWebTokenHandler _handler;

    public ClaimServices()
    {
        _handler = new JsonWebTokenHandler();
    }

    public async Task<IEnumerable<Claim>> ExtractClaimsFromEVEToken([NotNull] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentNullException(nameof(token));

        try
        {
            var securityToken = _handler.ReadJsonWebToken(token);
            if (securityToken == null)
                throw new InvalidOperationException("Invalid JWT token.");

            var claims = new List<Claim>(securityToken.Claims);

            var nameClaim = await ExtractClaim(securityToken, "name");
            //var expClaim = await ExtractClaim(securityToken, "exp");

            claims.Add(new Claim(ClaimTypes.NameIdentifier, securityToken.Subject.Replace("CHARACTER:EVE:", string.Empty, StringComparison.OrdinalIgnoreCase), ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
            claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value, ClaimValueTypes.String, EVEOnlineAuthenticationDefaults.Issuer));
            //claims.Add(new Claim(ClaimTypes.Expiration, UnixTimeStampToDateTime(expClaim.Value), ClaimValueTypes.DateTime, EVEOnlineAuthenticationDefaults.Issuer));

            var scopes = claims.Where(x => string.Equals(x.Type, "scp", StringComparison.OrdinalIgnoreCase)).ToList();
            if (scopes.Any())
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

    private Task<Claim> ExtractClaim([NotNull] JsonWebToken token, [NotNull] string claim)
    {
        var extractedClaim = token.Claims.FirstOrDefault(x => string.Equals(x.Type, claim, StringComparison.OrdinalIgnoreCase));
        if (extractedClaim == null)
        {
            throw new InvalidOperationException($"The claim '{claim}' is missing from the EVEOnline JWT.");
        }

        return Task.FromResult(extractedClaim);
    }
}

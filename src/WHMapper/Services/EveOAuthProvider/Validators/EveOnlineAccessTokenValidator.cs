using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace WHMapper.Services.EveOAuthProvider.Validators;

public class EveOnlineAccessTokenValidator : IEveOnlineAccessTokenValidator
{
    private readonly EVEOnlineAuthenticationOptions _options;
    private readonly ILogger<EveOnlineAccessTokenValidator> _logger;

    public EveOnlineAccessTokenValidator(IOptionsMonitor<EVEOnlineAuthenticationOptions> options, ILogger<EveOnlineAccessTokenValidator> logger)
    {
        _options = options.Get(EVEOnlineAuthenticationDefaults.AuthenticationScheme);
        _logger = logger;
    }


    public async Task<bool> ValidateAsync(string accessToken)
    {
        try
        {
            var response = await _options.Backchannel.GetStringAsync(_options.JWKEndpoint);
            var jwk = new JsonWebKeySet(response).Keys.First();

            TokenValidationParameters tokenValidationParams = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,

                ValidAudience = EVEOnlineAuthenticationDefaults.ValideAudience,
                ValidIssuer = EVEOnlineAuthenticationDefaults.ValideIssuer,
                IssuerSigningKey = jwk,
                ClockSkew = TimeSpan.FromSeconds(2), // CCP's servers seem slightly ahead (~1s)
            };

            var validationResult = await new JwtSecurityTokenHandler().ValidateTokenAsync(accessToken, tokenValidationParams);


            if(!validationResult.IsValid)
            {
                _logger.LogWarning(validationResult.Exception,"Access token is invalid");

            }
            return validationResult.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate access token");
            return false;
        }
    }
}

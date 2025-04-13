using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;


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
                ValidAudience = EVEOnlineAuthenticationDefaults.ValideAudience,

                ValidateIssuer = true,
                ValidIssuer = EVEOnlineAuthenticationDefaults.ValideIssuer,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwk,

                ClockSkew = TimeSpan.FromSeconds(2), // CCP's servers seem slightly ahead (~1s)
            };


            var validationResult = await new JsonWebTokenHandler().ValidateTokenAsync(accessToken, tokenValidationParams);


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

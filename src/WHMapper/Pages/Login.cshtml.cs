using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        private readonly IEveMapperUserManagementService _userManagementService;
        public LoginModel(ILogger<LoginModel> logger, IEveMapperUserManagementService userManagementService)
        {
            _logger = logger;
            _userManagementService = userManagementService;
        }

        public IActionResult OnGet(string redirectUri,string clientId)
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("./Login", pageHandler: "Callback", values: new { redirectUri, clientId })
            };

            return new ChallengeResult(EVEOnlineAuthenticationDefaults.AuthenticationScheme, authenticationProperties);
        }

        public async Task<IActionResult> OnGetCallback(string returnUrl = null, string remoteError = null,string clientId = null)
        {
 
            if (remoteError != null)
            {
                _logger.LogError($"Error from external provider: {remoteError}");
                return RedirectToPage("./Error");
            }

            var result = await HttpContext.AuthenticateAsync(EVEOnlineAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToPage("./Error");
            }

            var accountId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var accessToken = result.Properties.GetTokenValue("access_token");
            var refreshToken = result.Properties.GetTokenValue("refresh_token");
            var expiresAt = result.Properties.GetTokenValue("expires_at");

            if (!DateTimeOffset.TryParse(expiresAt, out var accessTokenExpiration))
            {
                return RedirectToPage("./Error");
            }

            var token = new UserToken
            {
                AccountId = accountId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiry = accessTokenExpiration.UtcDateTime
            };

            await _userManagementService.AddAuthenticateWHMapperUser(clientId, accountId, token);

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}

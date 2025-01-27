using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;
using YamlDotNet.Core.Tokens;

namespace WHMapper.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly IEveOnlineTokenProvider _tokenProvider;


        public LoginModel(ILogger<LogoutModel> logger, IEveOnlineTokenProvider tokenProvider)
        {
            _logger = logger;
            _tokenProvider = tokenProvider;
        }


        public IActionResult OnGet(string redirectUri,bool addionnalAccount=false)
        {
            // Request a redirect to the external login provider.
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("./Login",
                pageHandler: "Callback",
                values: new { redirectUri }),
            };

            return new ChallengeResult(EVEOnlineAuthenticationDefaults.AuthenticationScheme, authenticationProperties);
        }

        public async Task<IActionResult> OnGetCallback(
            string returnUrl = null!, string remoteError = null!)
        {
            var result = await HttpContext.AuthenticateAsync(EVEOnlineAuthenticationDefaults.AuthenticationScheme);

            var accountId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Or any unique identifier
            var accessToken = result.Properties.GetTokenValue("access_token");
            var refreshToken = result.Properties.GetTokenValue("refresh_token");
            var expiry = DateTime.UtcNow.AddSeconds(double.Parse(result.Properties.GetTokenValue("expires_in") ?? "0"));
            
            var token = new UserToken
            {
                AccountId = accountId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiry = expiry
            };

            await _tokenProvider.SaveToken(token);
            return LocalRedirect("/");
        }


    }
}

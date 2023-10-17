using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LoginModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnGetAsync(string redirectUri)
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

        public async Task<IActionResult> OnGetCallbackAsync(
            string returnUrl = null, string remoteError = null)
        {
            // Get the information about the user from the external login provider
            var EveUser = this.User.Identities.FirstOrDefault();
            if (EveUser!=null && EveUser.IsAuthenticated)
            {
                _logger.LogInformation("User authenticated");


            }
            return LocalRedirect("/");
        }

    
    }
}

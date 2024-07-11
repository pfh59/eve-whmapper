using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Shared.Services.EveOAuthProvider;

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

        public IActionResult OnGet(string redirectUri)
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

        public IActionResult OnGetCallback(
            string returnUrl = null!, string remoteError = null!)
        {
            // Get the information about the user from the external login provider
            var EveUser = this.User.Identities.FirstOrDefault();
            if (EveUser != null && EveUser.IsAuthenticated)
            {
                _logger.LogInformation("User authenticated");


            }
            return LocalRedirect("/");
        }


    }
}

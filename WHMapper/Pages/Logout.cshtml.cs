using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            _logger.LogInformation("User logged out.");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(new AuthenticationProperties { RedirectUri = "/" });
            //await HttpContext.SignOutAsync(EVEOnlineAuthenticationDefaults.AuthenticationScheme,new AuthenticationProperties { RedirectUri = "/" });
        }

        /*
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(new AuthenticationProperties { RedirectUri = "/" });
            
            _logger.LogInformation("User logged out.");
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect the user to application root
                return LocalRedirect("~/");
            }
        }*/
    }
}

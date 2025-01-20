using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;
namespace WHMapper.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly EVEOnlineTokenProvider _tokenProvider;

        public LogoutModel(ILogger<LogoutModel> logger, EVEOnlineTokenProvider tokenProvider)
        {
            _logger = logger;
            _tokenProvider = tokenProvider;
        }

        public async Task<IActionResult> OnGetAsync()
        { 
            try
            {
                await _tokenProvider.RevokeToken();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User logged out.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Logout Error");
            }
            return LocalRedirect("/");
        }
    }
}

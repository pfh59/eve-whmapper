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
        private readonly IEveOnlineTokenProvider _tokenProvider;

        public LogoutModel(ILogger<LogoutModel> logger, IEveOnlineTokenProvider tokenProvider)
        {
            _logger = logger;
            _tokenProvider = tokenProvider;
        }

        public async Task<IActionResult> OnGetAsync()
        { 
            try
            {
                var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await _tokenProvider.RevokeToken(userId);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await _tokenProvider.ClearToken(userId);
                
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

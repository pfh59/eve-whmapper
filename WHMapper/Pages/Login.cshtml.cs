using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LoginModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet(string redirectUri)
        {
            _logger.LogInformation("User ask authentication");
            await HttpContext.ChallengeAsync(EVEOnlineAuthenticationDefaults.AuthenticationScheme,new AuthenticationProperties
            {
                RedirectUri = redirectUri
            });
        }
    }
}

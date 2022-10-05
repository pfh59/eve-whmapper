using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Pages
{
    public class LoginModel : PageModel
    {
        public async Task OnGet(string redirectUri)
        {

            await HttpContext.ChallengeAsync(EVEOnlineAuthenticationDefaults.AuthenticationScheme,new AuthenticationProperties
            {
                RedirectUri = redirectUri
            });

           
        }
    }
}

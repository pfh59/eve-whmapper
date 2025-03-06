using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveOAuthProvider.Services;
using WHMapper.Services.LocalStorage;
namespace WHMapper.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly IEveMapperUserManagementService _userManagementService;
      
        public LogoutModel(ILogger<LogoutModel> logger,ClientUID UID, IEveMapperUserManagementService userManagementService)
        {
            _logger = logger;
            _userManagementService = userManagementService;
        }


        public async Task<IActionResult> OnGetAsync(string? clientId=null)
        { 
            try
            {
                var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
  
                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(userId))
                {
                    await _userManagementService.RemoveAuthenticateWHMapperUser(clientId, userId);
                }
  
                await HttpContext.SignOutAsync();            
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

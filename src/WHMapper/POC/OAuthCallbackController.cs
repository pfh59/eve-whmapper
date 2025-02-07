using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WHMapper.POC
{
    [Route("oauth-callback")]
    [ApiController]
    public class OAuthCallbackController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public OAuthCallbackController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Callback(string code, string state)
        {
                    using var client = _httpClientFactory.CreateClient();

        var tokenRequest = new Dictionary<string, string>
        {
            { "client_id", "your-client-id" },
            { "client_secret", "your-client-secret" },
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", "https://yourapp.com/oauth-callback" }
        };

        var response = await client.PostAsync("https://oauth-provider.com/token", new FormUrlEncodedContent(tokenRequest));
        var tokens = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        var sessionId = state; // Use `state` as session identifier
        //_tokenService.StoreToken(sessionId, tokens["access_token"], tokens["refresh_token"]);

        return Content("<script>window.parent.postMessage('auth-success', '*');</script>", "text/html");
        }
    }
}

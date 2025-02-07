using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WHMapper.POC
{
    [Route("api/oauth-proxy")]
    [ApiController]
    public class OAuthProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public OAuthProxyController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] OAuthRequest request)
        {
            var client_id = "39eee319b2d14a4a9ba3e9eb1b1410b8";
            var client_secret = "Qg5uFfjYcqhvrw6FdXhxTMHM9yxNBBb8N3BYGvCN";
            var scope = "esi-location.read_location.v1 esi-location.read_ship_type.v1 esi-ui.open_window.v1 esi-ui.write_waypoint.v1 esi-search.search_structures.v1";


            var tokenRequest = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret},
                { "code", request.Code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", "https://localhost:5001/oauth-callback" }
            };

            var response = await _httpClient.PostAsync("https://your-oauth2-provider.com/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to get token");
            }

            var tokenData = await response.Content.ReadAsStringAsync();
            return Ok(tokenData);
        
        }
    }
}

public class OAuthRequest
{
    public string Code { get; set; }
}

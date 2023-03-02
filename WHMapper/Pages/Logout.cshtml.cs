using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WHMapper.Models.DTO;
using WHMapper.Services.EveOAuthProvider;
namespace WHMapper.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private const string revokendpoint = "https://login.eveonline.com/v2/oauth/revoke";
        private readonly ILogger<LogoutModel> _logger;

        

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configurationManager;

        private readonly IConfigurationSection? _evessoConf = null;
        private readonly HttpClient? _httpClient = null;
        

        public LogoutModel(IConfiguration configurationManager, IHttpClientFactory httpClientFactory,ILogger<LogoutModel> logger)
        {
            _logger = logger;
            _configurationManager = configurationManager;
            _httpClientFactory = httpClientFactory;

           
            _evessoConf = _configurationManager.GetSection("EveSSO");
            string _clientKey = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_evessoConf["ClientId"]}:{_evessoConf["Secret"]}"));

            if (_httpClient == null)
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri("https://login.eveonline.com");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _clientKey);
                _httpClient.DefaultRequestHeaders.Host = "login.eveonline.com";
            }      
        }

            
        public async Task OnGet()
        { 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await RevokeTokken();
            _logger.LogInformation("User logged out.");
        }

        private async Task RevokeTokken()
        {
            string accessToken = await HttpContext.GetTokenAsync(EVEOnlineAuthenticationDefaults.AuthenticationScheme, "access_token");
            string refreshToken = await HttpContext.GetTokenAsync(EVEOnlineAuthenticationDefaults.AuthenticationScheme, "refresh_token");


            var body = $"token_type_hint=refresh_token&token={Uri.EscapeDataString(refreshToken)}";

            HttpContent postBody = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync(revokendpoint, postBody);

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {

                var error = JsonSerializerExtensions.DeserializeAnonymousType(content, new { error_description = string.Empty }).error_description;
                throw new ArgumentException(error);
            }
        }

        public static partial class JsonSerializerExtensions
        {
            public static T? DeserializeAnonymousType<T>(string json, T anonymousTypeObject, JsonSerializerOptions? options = default)
                => JsonSerializer.Deserialize<T>(json, options);

            public static ValueTask<TValue?> DeserializeAnonymousTypeAsync<TValue>(Stream stream, TValue anonymousTypeObject, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
                => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken); // Method to deserialize from a stream added for completeness
        }
    }
}

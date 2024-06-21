using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.SSO;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveJwtAuthenticationStateProvider
{

    public class EveAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenProvider _tokkenInfo;
        private readonly IConfiguration _configurationManager;

        private readonly IConfigurationSection? _evessoConf = null;
        private readonly HttpClient? _httpClient = null;
        private readonly string _clientKey;



        public EveAuthenticationStateProvider(IConfiguration configurationManager, IHttpClientFactory httpClientFactory, TokenProvider tokkenInfo) : base()
        {
            _configurationManager = configurationManager;
            _httpClientFactory = httpClientFactory;
            _tokkenInfo = tokkenInfo;


            _evessoConf = _configurationManager.GetSection("EveSSO");
            _clientKey = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_evessoConf["ClientId"]}:{_evessoConf["Secret"]}"));

            if (_httpClient == null)
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri("https://login.eveonline.com");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _clientKey);
                _httpClient.DefaultRequestHeaders.Host = "login.eveonline.com";
            }

        }


        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            if (_tokkenInfo == null || string.IsNullOrWhiteSpace(_tokkenInfo.AccessToken) || string.IsNullOrWhiteSpace(_tokkenInfo.RefreshToken))
                return anonymousState;


            if (await IsTokenExpired())//auto renew token
            {
                EveToken? newEveToken = await RenewToken();

                if (newEveToken == null)
                    return anonymousState;
                else
                {
                    _tokkenInfo.AccessToken = newEveToken.AccessToken;
                    _tokkenInfo.RefreshToken = newEveToken.RefreshToken;
                }
            }

            var claims = EVEOnlineAuthenticationHandler.ExtractClaimsFromEVEToken(_tokkenInfo.AccessToken);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }


        private Task<bool> IsTokenExpired()
        {
            JsonWebTokenHandler SecurityTokenHandle = new JsonWebTokenHandler();
            var securityToken = SecurityTokenHandle.ReadJsonWebToken(_tokkenInfo.AccessToken);
            var expiry = EVEOnlineAuthenticationHandler.ExtractClaim(securityToken, "exp");

            if (expiry == null)
            {
                return Task.FromResult(true);
            }



            var datetime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
            if (datetime.UtcDateTime <= DateTime.UtcNow)
            {
                return Task.FromResult(true);
            }


            return Task.FromResult(false);
        }


        private async Task<EveToken?> RenewToken()
        {
            if (_httpClient == null)
            {
                return null;
            }

            if (_tokkenInfo == null || string.IsNullOrWhiteSpace(_tokkenInfo.RefreshToken))
            {
                return null;
            }


            var body = $"grant_type=refresh_token&refresh_token={Uri.EscapeDataString(_tokkenInfo.RefreshToken)}";
            HttpContent postBody = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");


            var response = await _httpClient.PostAsync(EVEOnlineAuthenticationDefaults.TokenEndpoint, postBody);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            else
            {
                string result = response.Content.ReadAsStringAsync().Result;
                return JsonSerializer.Deserialize<EveToken>(result);
            }
        }


    }
}




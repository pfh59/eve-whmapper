using Microsoft.AspNetCore.Authentication;
using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO.EveAPI.SSO;

namespace WHMapper.Services.EveOAuthProvider.Middleware;

public class EveTokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configurationManager;



    public EveTokenRefreshMiddleware(IConfiguration configurationManager, IHttpClientFactory httpClientFactory,RequestDelegate next)
    {
        _configurationManager = configurationManager;
        _httpClientFactory = httpClientFactory;
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if(context.User.Identity.IsAuthenticated)
        {
            var tokenExpiration = await context.GetTokenAsync("expires_at");
            if(tokenExpiration != null)
            {
                var expirationDate = DateTime.Parse(tokenExpiration, null, DateTimeStyles.RoundtripKind);
                if (expirationDate < DateTime.UtcNow.AddMinutes(-5)) // Rafraîchir avant expiration
                    {
                        var refreshToken = await context.GetTokenAsync("refresh_token");
                        if (refreshToken != null)
                        {
                            // Appelez votre fournisseur d'identité pour obtenir un nouveau token
                            var newToken = await RefreshAccessToken(refreshToken);
                           // context.Response.Cookies.Append("access_token", newToken.AccessToken);
                            // Mettre à jour le cookie d'authentification
                        }
                }
            }
            

        }
        await _next(context);
    }

    private async Task<EveToken?> RefreshAccessToken(string refreshToken)
    {
        var evessoConf = _configurationManager.GetSection("EveSSO");
        var clientKey = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{evessoConf["ClientId"]}:{evessoConf["Secret"]}"));
        
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://login.eveonline.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", clientKey);
        client.DefaultRequestHeaders.Host = "login.eveonline.com";
        
        var response = await client.PostAsync(EVEOnlineAuthenticationDefaults.TokenEndpoint, new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string?, string?>("grant_type", "refresh_token"),
            new KeyValuePair<string?, string?>("refresh_token", Uri.EscapeDataString(refreshToken))
        }));

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EveToken>(result);




        //var body = $"grant_type=refresh_token&refresh_token={Uri.EscapeDataString(_tokenInfo.RefreshToken)}";
        //HttpContent postBody = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

        //var response = await _httpClient.PostAsync(EVEOnlineAuthenticationDefaults.TokenEndpoint, postBody);


    }

        private Task<bool> IsTokenExpired()
        {
            /*
            JsonWebTokenHandler SecurityTokenHandle = new JsonWebTokenHandler();
            var securityToken = SecurityTokenHandle.ReadJsonWebToken(_tokenInfo.AccessToken);
            var expiry = EVEOnlineAuthenticationHandler.ExtractClaim(securityToken, "exp");

            if (expiry == null)
            {
                return Task.FromResult(true);
            }

            var datetime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
            if (datetime.UtcDateTime <= DateTime.UtcNow)
            {
                return Task.FromResult(true);
            }*/

            return Task.FromResult(false);
        }


}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO.EveAPI.SSO;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveOAuthProvider.Services;

public class EVEOnlineTokenProvider
{
    private readonly IConfiguration _configurationManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? TokenExpiration { get; set; }

    public EVEOnlineTokenProvider(IConfiguration configurationManager,IHttpClientFactory httpClientFactory,IHttpContextAccessor httpContextAccessor)
    {
        _configurationManager = configurationManager;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        
        InitializeTokens().GetAwaiter().GetResult();        
    }
    private async Task InitializeTokens()
    {
        // Assuming tokens are passed as headers or cookies
        var context = _httpContextAccessor.HttpContext;

        if (context != null)
        {
            AccessToken = await context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme,"access_token");
            RefreshToken = await context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme,"refresh_token");
            var exp_at = await context.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme,"expires_at");
            if(DateTimeOffset.TryParse(exp_at, out var exp))
            {
                TokenExpiration = exp;
            }
        }
    }

    public Task<bool> IsTokenExpire()
    {
        if (TokenExpiration == null)
            throw new NullReferenceException("Token expiration is null");
     
        return Task.FromResult(DateTimeOffset.UtcNow > TokenExpiration.Value.AddMinutes(-5));
    }

    public async Task RefreshAccessToken()
    {
        if(string.IsNullOrEmpty(RefreshToken))
            throw new NullReferenceException("Refresh token is null or empty");
        

        var evessoConf = _configurationManager.GetSection("EveSSO");
        var clientKey = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{evessoConf["ClientId"]}:{evessoConf["Secret"]}"));
        
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://login.eveonline.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", clientKey);
        client.DefaultRequestHeaders.Host = "login.eveonline.com";
        

        var body = $"grant_type=refresh_token&refresh_token={Uri.EscapeDataString(RefreshToken)}";
        HttpContent postBody = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await client.PostAsync(EVEOnlineAuthenticationDefaults.TokenEndpoint, postBody);
        

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        EveToken? newToken = JsonSerializer.Deserialize<EveToken>(result);

        if(newToken == null)
            throw new NullReferenceException("New token is null");
        

        AccessToken = newToken.AccessToken;
        RefreshToken = newToken.RefreshToken;
        TokenExpiration = DateTimeOffset.UtcNow.AddSeconds(newToken.ExpiresIn);
    }

}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.SSO;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveOAuthProvider.Services;

public class EVEOnlineTokenProvider
{
    private readonly ILogger<EVEOnlineTokenProvider> _logger;
    private readonly EVEOnlineAuthenticationOptions _options;
    private readonly HttpContext? context;

    public string? AccessToken {get;  set;}
    public string? RefreshToken {get;  set;}
    public DateTimeOffset? TokenExpiration {get;  set;}

    public EVEOnlineTokenProvider(ILogger<EVEOnlineTokenProvider> logger, IOptionsMonitor<EVEOnlineAuthenticationOptions> options, IHttpContextAccessor? context = null)
    {
        _logger = logger;
        _options =  options.Get(CookieAuthenticationDefaults.AuthenticationScheme);
        this.context = context?.HttpContext;

        InitializeTokens().GetAwaiter().GetResult();  
    }

     private async Task InitializeTokens()
    {
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
        

        var refreshToken = Uri.EscapeDataString(RefreshToken);
        var content = new StringContent($"grant_type=refresh_token&refresh_token={RefreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

        _options.Backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
        var response = await _options.Backchannel.PostAsync(_options.TokenEndpoint, content);
        

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        EveToken? newToken = JsonSerializer.Deserialize<EveToken>(result);

        if(newToken == null)
            throw new NullReferenceException("New token is null");


        AccessToken = newToken.AccessToken;
        RefreshToken = newToken.RefreshToken;
        TokenExpiration = DateTimeOffset.UtcNow.AddSeconds(newToken.ExpiresIn);
    }

    public async Task RevokeToken()
    {
        if(string.IsNullOrEmpty(RefreshToken))
            throw new NullReferenceException("Refresh token is null or empty");

        var refreshToken = Uri.EscapeDataString(RefreshToken);
        var content = new StringContent($"token_type_hint=refresh_token&token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

        _options.Backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
        var response = await _options.Backchannel.PostAsync(_options.RevokeTokenEndpoint, content);

        response.EnsureSuccessStatusCode();
    }
}

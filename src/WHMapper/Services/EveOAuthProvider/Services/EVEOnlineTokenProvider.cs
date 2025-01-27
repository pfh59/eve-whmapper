using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.SSO;
using WHMapper.Services.EveOAuthProvider;

namespace WHMapper.Services.EveOAuthProvider.Services;

public class EveOnlineTokenProvider : IEveOnlineTokenProvider
{
    private readonly EVEOnlineAuthenticationOptions _options;
    private readonly ConcurrentDictionary<string, UserToken> _tokens = new();


    public EveOnlineTokenProvider(IOptionsMonitor<EVEOnlineAuthenticationOptions> options, IHttpContextAccessor? context = null)
    {
        _options =  options.Get(EVEOnlineAuthenticationDefaults.AuthenticationScheme);
    }

    public Task SaveToken(UserToken token)
    {
        if (token.AccountId != null)
        {
            if(_tokens.ContainsKey(token.AccountId))
            {
                _tokens[token.AccountId] = token;
            }
            else
            {
                _tokens.TryAdd(token.AccountId, token);
            }
        }
        else
        {
            throw new ArgumentNullException(nameof(token.AccountId), "AccountId cannot be null");
        }
        return Task.CompletedTask;
    }

    public Task<UserToken?> GetToken(string accountId)
    {
        return Task.FromResult(_tokens.TryGetValue(accountId, out var token) ? token : null);
    }

    public Task ClearToken(string accountId)
    {
        _tokens.TryRemove(accountId, out _);
        return Task.CompletedTask;
    }

    public async Task<bool> IsTokenExpire(string accountId)
    {
        var token = await GetToken(accountId);
        if(token == null)
            throw new NullReferenceException("Token is null");
    
        return DateTimeOffset.UtcNow > token.Expiry.AddMinutes(-18);
    }

    public async Task RefreshAccessToken(string accountId)
    {
        var token = await GetToken(accountId);
        if(token == null)
            throw new NullReferenceException("Token is null");

        var refreshToken = Uri.EscapeDataString(token.RefreshToken);
        var content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

        _options.Backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
        var response = await _options.Backchannel.PostAsync(_options.TokenEndpoint, content);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        EveToken? newToken = JsonSerializer.Deserialize<EveToken>(result);

        if(newToken == null)
            throw new NullReferenceException("New token is null");

        token.AccessToken = newToken.AccessToken;
        token.RefreshToken = newToken.RefreshToken;
        token.Expiry = DateTimeOffset.UtcNow.AddSeconds(newToken.ExpiresIn).UtcDateTime;

        await SaveToken(token);
    }

    /*
    public async Task RevokeToken()
    {
        if(string.IsNullOrEmpty(RefreshToken))
            throw new NullReferenceException("Refresh token is null or empty");

        var refreshToken = Uri.EscapeDataString(RefreshToken);
        var content = new StringContent($"token_type_hint=refresh_token&token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

        _options.Backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
        var response = await _options.Backchannel.PostAsync(_options.RevokeTokenEndpoint, content);

        response.EnsureSuccessStatusCode();
    }*/
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.SSO;

namespace WHMapper.Services.EveOAuthProvider.Services
{
    public class EveOnlineTokenProvider : IEveOnlineTokenProvider
    {
        private readonly EVEOnlineAuthenticationOptions _options;
        private readonly ConcurrentDictionary<string, UserToken> _tokens = new();


        public EveOnlineTokenProvider(IOptionsMonitor<EVEOnlineAuthenticationOptions> options)
        {
            _options = options.Get(EVEOnlineAuthenticationDefaults.AuthenticationScheme);
        }
        
        public async Task SaveToken(UserToken token)
        {
            if (token.AccountId == null)
            {
                throw new ArgumentNullException(nameof(token.AccountId), "AccountId cannot be null");
            }

            if(_tokens.ContainsKey(token.AccountId))
            {
                while (!_tokens.Remove(token.AccountId, out _))
                    await Task.Delay(1);

            }
           
            while(! _tokens.TryAdd(token.AccountId, token))
                await Task.Delay(1);
            
        }

        public async Task<UserToken?> GetToken(string accountId, bool autoResfred = false)
        {
            _tokens.TryGetValue(accountId, out var token);
            if(autoResfred && token != null && await IsTokenExpire(token))
            {
                await RefreshAccessToken(accountId);
                _tokens.TryGetValue(accountId, out token);
            }
            return token;
        }

        public Task ClearToken(string accountId)
        {
            _tokens.TryRemove(accountId, out _);
            return Task.CompletedTask;
        }

        public async Task<bool> IsTokenExpire(string accountId)
        {
            var token = await GetToken(accountId);
            if (token == null)
            {
                throw new NullReferenceException("Token is null");
            }
            return await IsTokenExpire(token);
        }

        private Task<bool> IsTokenExpire(UserToken token)
        {
            if (token == null)
            {
                throw new NullReferenceException("Token is null");
            }

            return Task.FromResult(DateTimeOffset.UtcNow > token.Expiry.AddMinutes(-5));//// 5 minutes before expiry
        }

        public async Task RefreshAccessToken(string accountId)
        {
            var token = await GetToken(accountId);
            if (token == null)
            {
                throw new NullReferenceException("Token is null");
            }

            var refreshToken = Uri.EscapeDataString(token.RefreshToken);
            var content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

            _options.Backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
            var response = await _options.Backchannel.PostAsync(_options.TokenEndpoint, content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var newToken = JsonSerializer.Deserialize<EveToken>(result);

            if (newToken == null)
            {
                throw new NullReferenceException("New token is null");
            }

            token.AccessToken = newToken.AccessToken;
            token.RefreshToken = newToken.RefreshToken;
            token.Expiry = DateTimeOffset.UtcNow.AddSeconds(newToken.ExpiresIn).UtcDateTime;

            await SaveToken(token);

            _options.Backchannel.DefaultRequestHeaders.Authorization=null;
        }

        public async Task RevokeToken(string accountId)
        {
            var token = await GetToken(accountId);
            if (token == null)
            {
                throw new NullReferenceException("Token is null");
            }

            var refreshToken = Uri.EscapeDataString(token.RefreshToken);
            var content = new StringContent($"token_type_hint=refresh_token&token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

            _options.Backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
            var response = await _options.Backchannel.PostAsync(_options.RevokeTokenEndpoint, content);

            _options.Backchannel.DefaultRequestHeaders.Authorization=null;
            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync());
                throw new ArgumentException(error?.ErrorDescription);
            }

            await ClearToken(accountId);
        }

        private class ErrorResponse
        {
            public string ErrorDescription { get; set; }
        }
    }
}

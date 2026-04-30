using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.SSO;

namespace WHMapper.Services.EveOAuthProvider.Services
{
    public class EveOnlineTokenProvider : IEveOnlineTokenProvider
    {
        private readonly EVEOnlineAuthenticationOptions _options;
        private readonly ConcurrentDictionary<string, UserToken> _tokens = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _refreshLocks = new();


        public EveOnlineTokenProvider(IOptionsMonitor<EVEOnlineAuthenticationOptions> options)
        {
            _options = options.Get(EVEOnlineAuthenticationDefaults.AuthenticationScheme);
        }
        
        public Task SaveToken(UserToken token)
        {
            if (token.AccountId == null)
            {
                throw new ArgumentNullException(nameof(token.AccountId), "AccountId cannot be null");
            }

            _tokens.AddOrUpdate(token.AccountId, token, (_, _) => token);
            return Task.CompletedTask;
        }

        public async Task<UserToken?> GetToken(string accountId, bool autoRefreshed = false)
        {
            _tokens.TryGetValue(accountId, out var token);
            if(autoRefreshed && token != null && await IsTokenExpire(token))
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
            var semaphore = _refreshLocks.GetOrAdd(accountId, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
            var token = await GetToken(accountId);
            if (token == null)
            {
                throw new NullReferenceException("Token is null");
            }

            // Double-check: another thread may have already refreshed while we were waiting
            if (!await IsTokenExpire(token))
            {
                return;
            }

            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                throw new InvalidOperationException("Refresh token is missing.");
            }

            var refreshToken = Uri.EscapeDataString(token.RefreshToken);
            var content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
            var response = await _options.Backchannel.SendAsync(request);

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
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task RevokeToken(string accountId)
        {
            var token = await GetToken(accountId);
            if (token == null)
            {
                throw new NullReferenceException("Token is null");
            }

            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                throw new InvalidOperationException("Refresh token is missing.");
            }

            var refreshToken = Uri.EscapeDataString(token.RefreshToken);
            var content = new StringContent($"token_type_hint=refresh_token&token={refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, _options.RevokeTokenEndpoint) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
            var response = await _options.Backchannel.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync());
                throw new ArgumentException(error?.ErrorDescription);
            }

            await ClearToken(accountId);
        }

        private class ErrorResponse
        {
            [JsonPropertyName("error_description")]
            public string? ErrorDescription { get; set; }
        }
    }
}

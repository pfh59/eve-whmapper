using System.Net.Http.Headers;
using System.Security.Claims;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Services.EveAPI
{
    public class EveOnlineAccessTokenHandler : DelegatingHandler
    {
        private readonly IEveOnlineTokenProvider _eveAPITokenProvider;
        private readonly IHttpContextAccessor _contextAccessor;

        public EveOnlineAccessTokenHandler(IEveOnlineTokenProvider eveAPITokenProvider, IHttpContextAccessor contextAccessor)
        {
            _eveAPITokenProvider = eveAPITokenProvider ?? throw new ArgumentNullException(nameof(eveAPITokenProvider));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
        
            var user = _contextAccessor?.HttpContext?.User as ClaimsPrincipal;

 
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID is not found.");
            }

            if (await _eveAPITokenProvider.IsTokenExpire(userId))
            {
                await _eveAPITokenProvider.RefreshAccessToken(userId);
            }

            var token = (await _eveAPITokenProvider.GetToken(userId))?.AccessToken;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

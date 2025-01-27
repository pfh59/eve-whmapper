using System.Net.Http.Headers;
using System.Security.Claims;
using WHMapper.Services.EveOAuthProvider.Services;



namespace WHMapper.Services.EveAPI;
public class EveOnlineAccessTokenHandler : DelegatingHandler
{
    private readonly IEveOnlineTokenProvider _eveAPITokenProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EveOnlineAccessTokenHandler(IEveOnlineTokenProvider eveAPITokenProvider,IHttpContextAccessor httpContextAccessor)
    {
        _eveAPITokenProvider = eveAPITokenProvider;
        _httpContextAccessor = httpContextAccessor;
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new Exception("User is not authenticated.");
        }  


        if(await _eveAPITokenProvider.IsTokenExpire(userId))
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

using System.Net.Http.Headers;
using WHMapper.Services.EveOAuthProvider.Services;



namespace WHMapper.Services.EveAPI;
public class EveOnlineAccessTokenHandler : DelegatingHandler
{
    private readonly EVEOnlineTokenProvider _eveAPITokenProvider;

    public EveOnlineAccessTokenHandler(EVEOnlineTokenProvider eveAPITokenProvider)
    {
        _eveAPITokenProvider = eveAPITokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if(await _eveAPITokenProvider.IsTokenExpire())
        {
            await _eveAPITokenProvider.RefreshAccessToken();
        }
        var token = _eveAPITokenProvider.AccessToken;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        } 
        
        return await base.SendAsync(request, cancellationToken);
    }
}

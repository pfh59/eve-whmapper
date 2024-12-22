using Microsoft.AspNetCore.Authentication;
using System;
using System.Net.Http.Headers;

namespace WHMapper.Services.EveOAuthProvider;

public class EveOnlineAccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EveOnlineAccessTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

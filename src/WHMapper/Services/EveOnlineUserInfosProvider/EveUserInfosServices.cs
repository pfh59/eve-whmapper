using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using WHMapper.Services.EveJwtAuthenticationStateProvider;


namespace WHMapper.Services.EveOnlineUserInfosProvider
{
	public class EveUserInfosServices : IEveUserInfosServices
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        
       
        public EveUserInfosServices(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<string> GetUserName()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return state?.User?.Identity?.Name ?? IEveUserInfosServices.ANONYMOUS_USERNAME;
        }

        public async Task<string> GetCharactedID()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return state?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty; ;
        }
    }
}


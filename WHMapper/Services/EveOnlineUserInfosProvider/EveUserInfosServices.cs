using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using WHMapper.Services.EveJwtAuthenticationStateProvider;


namespace WHMapper.Services.EveOnlineUserInfosProvider
{
	public class EveUserInfosServices : IEveUserInfosServices
    {
        private const string _defaultusername = "Anonymous";

        private readonly AuthenticationStateProvider _authenticationStateProvider;
        
       
        public EveUserInfosServices(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<string> GetUserName()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return state?.User?.Identity?.Name ?? _defaultusername;
        }

        public async Task<string> GetCharactedID()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return state?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty; ;
        }
    }
}


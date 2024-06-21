using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;


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

        public async Task<int> GetCharactedID()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var characterId = state?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(characterId))
                return 0;
            else
                return Convert.ToInt32(characterId);
        }
    }
}


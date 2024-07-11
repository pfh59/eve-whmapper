using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Services.EveAPI.Locations
{
    internal class LocationServices : EveApiServiceBase, ILocationServices
    {
        private readonly IEveUserInfosServices? _userService = null!;

        public LocationServices(HttpClient httpClient, TokenProvider _tokenProvider, IEveUserInfosServices userService) : base(httpClient, _tokenProvider)
        {
            _userService = userService;
        }

        public async Task<EveLocation?> GetLocation()
        {
            if (_userService != null)
            {
                int characterId = await _userService.GetCharactedID();
                return await base.Execute<EveLocation>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/location/?datasource=tranquility", characterId));

            }
            return null;
        }

        public async Task<Ship?> GetCurrentShip()
        {
            if (_userService != null)
            {
                int characterId = await _userService.GetCharactedID();
                return await base.Execute<Ship>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/ship/?datasource=tranquility", characterId));
            }
            return null;
        }
    }
}

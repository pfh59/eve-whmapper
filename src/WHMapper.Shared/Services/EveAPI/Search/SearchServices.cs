using WHMapper.Shared.Models.DTO;
using WHMapper.Shared.Models.DTO.EveAPI.Search;
using WHMapper.Shared.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Shared.Services.EveAPI.Search
{
    public class SearchServices : EveApiServiceBase, ISearchServices
    {
        private readonly IEveUserInfosServices? _userService = null!;

        public SearchServices(HttpClient httpClient, TokenProvider _tokenProvider, IEveUserInfosServices userService) : base(httpClient, _tokenProvider)
        {
            _userService = userService;
        }

        public async Task<SearchAllianceResults?> SearchAlliance(string searchValue, bool isStrict = false)
        {
            if (_userService != null)
            {
                int characterId = await _userService.GetCharactedID();
                return await Execute<SearchAllianceResults>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v3/characters/{0}/search/?datasource=tranquility&search={1}&categories=alliance&strict={2}", characterId, searchValue, isStrict));

            }
            return null;
        }

        public async Task<SearchCharacterResults?> SearchCharacter(string searchValue, bool isStrict = false)
        {
            if (_userService != null)
            {
                int characterId = await _userService.GetCharactedID();
                return await Execute<SearchCharacterResults>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v3/characters/{0}/search/?datasource=tranquility&search={1}&categories=character&strict={2}", characterId, searchValue, isStrict));

            }
            return null;
        }

        public async Task<SearchCoporationResults?> SearchCorporation(string searchValue, bool isStrict = false)
        {
            if (_userService != null)
            {
                int characterId = await _userService.GetCharactedID();
                return await Execute<SearchCoporationResults>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v3/characters/{0}/search/?datasource=tranquility&search={1}&categories=corporation&strict={2}", characterId, searchValue, isStrict));

            }
            return null;
        }
    }
}

using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Assets;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Services.EveAPI.Assets
{
    public class AssetsServices : EveApiServiceBase, IAssetsServices
    {
        private readonly IEveUserInfosServices? _userService = null!;

        public AssetsServices(HttpClient httpClient, TokenProvider _tokenProvider, IEveUserInfosServices userService) : base(httpClient, _tokenProvider)
        {
            _userService = userService;
        }

        public async Task<ICollection<Asset>?> GetCharacterAssets(int character_id, int page = 1)
        {
            return await base.Execute<ICollection<Asset>?>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v5/characters/{0}/assets/?datasource=tranquility&page{1}", character_id, page));
        }

        public async Task<ICollection<Asset>?> GetMyAssets(int page = 1)
        {
            if (_userService != null)
            {
                int characterId = await _userService.GetCharactedID();
                return await GetCharacterAssets(characterId, page);
            }
            return null;
        }
    }
}
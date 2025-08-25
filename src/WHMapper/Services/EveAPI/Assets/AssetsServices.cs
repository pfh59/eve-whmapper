using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Assets;


namespace WHMapper.Services.EveAPI.Assets
{
    public class AssetsServices : EveApiServiceBase, IAssetsServices
    {
        public AssetsServices(HttpClient httpClient,UserToken? userToken=null) 
            : base(httpClient, userToken)
        {
        }
        public async Task<ICollection<Asset>?> GetCharacterAssets(int character_id, int page = 1)
        {
            return await base.Execute<ICollection<Asset>?>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v5/characters/{0}/assets/?datasource=tranquility&page{1}", character_id, page));
        }

        public async Task<ICollection<Asset>?> GetMyAssets(int page = 1)
        {
            if (this.UserToken != null)
            {
                if (UserToken.AccountId != null)
                {
                    int character_id = Int32.Parse(UserToken.AccountId);
                    return await GetCharacterAssets(character_id, page);
                }
                return null;
            }
            return null;
        }
    }
}
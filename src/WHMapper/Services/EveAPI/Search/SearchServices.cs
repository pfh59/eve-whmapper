using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Search;

namespace WHMapper.Services.EveAPI.Search
{
    public class SearchServices : EveApiServiceBase, ISearchServices
    {
        public SearchServices(HttpClient httpClient,UserToken? userToken=null) : base(httpClient,userToken)
        {
        }

        public async Task<SearchAllianceResults?> SearchAlliance(string searchValue, bool isStrict = false)
        {
            if (this.UserToken != null)
            {
                return await base.Execute<SearchAllianceResults>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v3/characters/{0}/search/?datasource=tranquility&search={1}&categories=alliance&strict={2}", UserToken.AccountId, searchValue, isStrict));

            }
            return null;
        }

        public async Task<SearchCharacterResults?> SearchCharacter(string searchValue, bool isStrict = false)
        {
            if (this.UserToken != null)
            {
                return await base.Execute<SearchCharacterResults>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v3/characters/{0}/search/?datasource=tranquility&search={1}&categories=character&strict={2}", UserToken.AccountId, searchValue, isStrict));

            }
            return null;
        }

        public async Task<SearchCoporationResults?> SearchCorporation(string searchValue, bool isStrict = false)
        {
            if (this.UserToken != null)
            {
                return await base.Execute<SearchCoporationResults>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v3/characters/{0}/search/?datasource=tranquility&search={1}&categories=corporation&strict={2}", UserToken.AccountId, searchValue, isStrict));

            }
            return null;
        }
    }
}

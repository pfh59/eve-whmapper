using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveOAuthProvider.Services;

namespace WHMapper.Services.EveAPI.Locations
{
    public class LocationServices : EveApiServiceBase, ILocationServices
    {
        public LocationServices(HttpClient httpClient,UserToken? userToken=null) : base(httpClient,userToken)
        {

        }

        public async Task<EveLocation?> GetLocation()
        {
            if (this.UserToken != null)
            {
                return await base.Execute<EveLocation>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/location/?datasource=tranquility", UserToken.AccountId));

            }
            return null;
        }

        public async Task<Ship?> GetCurrentShip()
        {
            if (this.UserToken != null)
            {
                return await base.Execute<Ship>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/ship/?datasource=tranquility", UserToken.AccountId));
            }
            return null;
        }
    }
}

using System.Net;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Location;

namespace WHMapper.Services.EveAPI.Locations
{
    public class LocationServices : EveApiServiceBase, ILocationServices
    {
        public LocationServices(HttpClient httpClient, UserToken? userToken = null) : base(httpClient, userToken)
        {
        }

        public async Task<Result<EveLocation>> GetLocation()
        {
            if (this.UserToken != null)
            {
                return await base.Execute<EveLocation>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/location/?datasource=tranquility", UserToken.AccountId));
            }
            return Result<EveLocation>.Failure("UserToken is required for authenticated requests", (int)HttpStatusCode.Unauthorized);
        }

        public async Task<Result<Ship>> GetCurrentShip()
        {
            if (this.UserToken != null)
            {
                return await base.Execute<Ship>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/ship/?datasource=tranquility", UserToken.AccountId));
            }
            return Result<Ship>.Failure("UserToken is required for authenticated requests", (int)HttpStatusCode.Unauthorized);
        }
    }
}
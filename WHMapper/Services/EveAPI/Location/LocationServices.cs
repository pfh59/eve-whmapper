using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.ResponseMessage;

namespace WHMapper.Services.EveAPI.Location
{
    internal class LocationServices : AEveApiServices,ILocationServices
    {
        private readonly string _characterId;

        public LocationServices(HttpClient httpClient,string token,string characterId) : base(httpClient, token)
        {
            _characterId = characterId;
        }

        public async Task<EveLocation> GetLocation()
        {
            return await base.Execute<EveLocation>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/location/?datasource=tranquility", _characterId));
        }
        
    }
}

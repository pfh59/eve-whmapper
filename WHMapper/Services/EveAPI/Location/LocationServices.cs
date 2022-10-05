using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.ResponseMessage;

namespace WHMapper.Services.EveAPI.Location
{
    internal class LocationServices : AEveApiServices,ILocationServices
    {
        private readonly string _characterId;

        public LocationServices(HttpClient httpClient, TokenProvider _tokenProvider) : base(httpClient, _tokenProvider)
        {
            _characterId = _tokenProvider.CharacterId;
        }

        public async Task<EveLocation> GetLocation()
        {
            return await base.Execute<EveLocation>(RequestSecurity.Authenticated, RequestMethod.Get, string.Format("/v2/characters/{0}/location/?datasource=tranquility", _characterId));
        }
        
    }
}

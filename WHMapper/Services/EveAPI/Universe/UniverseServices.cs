using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Universe;

namespace WHMapper.Services.EveAPI.Universe
{
    internal class UniverseServices : AEveApiServices, IUniverseServices
    {
        public UniverseServices(HttpClient httpClient, TokenProvider _tokenProvider) : base(httpClient, _tokenProvider)
        {
        }

        public async Task<SolarSystem> GetSystem(int system_id)
        {
            return await base.Execute<SolarSystem>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v4/universe/systems/{0}/?datasource=tranquility", system_id));

        }

        public async Task<Star> GetStar(int star_id)
        {
            return await base.Execute<Star>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/universe/stars/{0}/?datasource=tranquility", star_id));

        }
    }
}

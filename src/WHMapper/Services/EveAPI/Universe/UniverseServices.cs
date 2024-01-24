using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Universe;

namespace WHMapper.Services.EveAPI.Universe
{
    public class UniverseServices : AEveApiServices, IUniverseServices
    {
        public UniverseServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<int[]> GetSystems()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/universe/systems/?datasource=tranquility");
        }

        public async Task<ESISolarSystem> GetSystem(int system_id)
        {
            return await base.Execute<ESISolarSystem>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v4/universe/systems/{0}/?datasource=tranquility", system_id));

        }

        public async Task<Star> GetStar(int star_id)
        {
            return await base.Execute<Star>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/universe/stars/{0}/?datasource=tranquility", star_id));

        }

        public async Task<Group> GetGroup(int group_id)
        {
            return await base.Execute<Group>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/universe/groups/{0}/?datasource=tranquility", group_id));

        }

        public async Task<int[]> GetGroups()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/universe/groups/?datasource=tranquility");
        }

        public async Task<Category> GetCategory(int category_id)
        {
            return await base.Execute<Category>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/universe/categories/{0}/?datasource=tranquility", category_id));

        }
        public async Task<int[]> GetCategories()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/universe/categories/?datasource=tranquility");
        }

        public async Task<Models.DTO.EveAPI.Universe.Type> GetType(int type_id)
        {
            return await base.Execute<Models.DTO.EveAPI.Universe.Type>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v3/universe/types/{0}/?datasource=tranquility", type_id));

        }
        public async Task<int[]> GetTypes()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/universe/types/?datasource=tranquility");
        }

        public async Task<Stargate> GetStargate(int stargate_id)
        {
            return await base.Execute<Stargate>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/universe/stargates/{0}/?datasource=tranquility", stargate_id));
        }

        public async Task<int[]> GetContellations()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/universe/constellations/?datasource=tranquility");

        }

        public async Task<Constellation> GetContellation(int constellatio_id)
        {
            return await base.Execute<Constellation>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/universe/constellations/{0}/?datasource=tranquility", constellatio_id));

        }

        public async Task<int[]> GetRegions()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/universe/regions/?datasource=tranquility");

        }

        public async Task<Region> GetRegion(int region_id)
        {
            return await base.Execute<Region>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/universe/regions/{0}/?datasource=tranquility", region_id));

        }
    }

}

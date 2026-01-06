using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Services.EveAPI.Universe
{
    public class UniverseServices : EveApiServiceBase, IUniverseServices
    {
        public UniverseServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<int[]>> GetSystems()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/universe/systems/?datasource=tranquility");
        }

        public async Task<Result<ESISolarSystem>> GetSystem(int system_id)
        {
            return await base.Execute<ESISolarSystem>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/systems/{0}/?datasource=tranquility", system_id));
        }

        public async Task<Result<Star>> GetStar(int star_id)
        {
            return await base.Execute<Star>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/stars/{0}/?datasource=tranquility", star_id));
        }

        public async Task<Result<Group>> GetGroup(int group_id)
        {
            return await base.Execute<Group>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/groups/{0}/?datasource=tranquility", group_id));
        }

        public async Task<Result<int[]>> GetGroups()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/universe/groups/?datasource=tranquility");
        }

        public async Task<Result<Category>> GetCategory(int category_id)
        {
            return await base.Execute<Category>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/categories/{0}/?datasource=tranquility", category_id));
        }

        public async Task<Result<int[]>> GetCategories()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/universe/categories/?datasource=tranquility");
        }

        public async Task<Result<Models.DTO.EveAPI.Universe.Type>> GetType(int type_id)
        {
            return await base.Execute<Models.DTO.EveAPI.Universe.Type>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/types/{0}/?datasource=tranquility", type_id));
        }

        public async Task<Result<int[]>> GetTypes()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/universe/types/?datasource=tranquility");
        }

        public async Task<Result<Stargate>> GetStargate(int stargate_id)
        {
            return await base.Execute<Stargate>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/stargates/{0}/?datasource=tranquility", stargate_id));
        }

        public async Task<Result<int[]>> GetContellations()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/universe/constellations/?datasource=tranquility");
        }

        public async Task<Result<Constellation>> GetConstellation(int constellatio_id)
        {
            return await base.Execute<Constellation>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/constellations/{0}/?datasource=tranquility", constellatio_id));
        }

        public async Task<Result<int[]>> GetRegions()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/universe/regions/?datasource=tranquility");
        }

        public async Task<Result<Region>> GetRegion(int region_id)
        {
            return await base.Execute<Region>(RequestSecurity.Public, RequestMethod.Get, string.Format("/universe/regions/{0}/?datasource=tranquility", region_id));
        }
    }
}
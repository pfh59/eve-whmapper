using WHMapper.Shared.Models.DTO.EveAPI.Dogma;

namespace WHMapper.Shared.Services.EveAPI.Dogma
{
    public class DogmaServices : EveApiServiceBase, IDogmaServices
    {
        public DogmaServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Models.DTO.EveAPI.Dogma.Attribute?> GetAttribute(int attribute_id)
        {
            return await Execute<Models.DTO.EveAPI.Dogma.Attribute>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/dogma/attributes/{0}/?datasource=tranquility", attribute_id));
        }

        public async Task<int[]?> GetAttributes()
        {
            return await Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/dogma/attributes/?datasource=tranquility");
        }

        public async Task<Effect?> GetEffect(int effect_id)
        {
            return await Execute<Effect>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/dogma/effects/{0}/?datasource=tranquility", effect_id));
        }

        public async Task<int[]?> GetEffects()
        {
            return await Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/dogma/effects/?datasource=tranquility");
        }
    }
}
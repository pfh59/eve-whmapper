using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Dogma;

namespace WHMapper.Services.EveAPI.Dogma
{
    public class DogmaServices : EveApiServiceBase, IDogmaServices
    {
        public DogmaServices(HttpClient httpClient, UserToken? userToken = null) : base(httpClient, userToken)
        {
        }

        public async Task<Result<Models.DTO.EveAPI.Dogma.Attribute>> GetAttribute(int attribute_id)
        {
            return await base.Execute<Models.DTO.EveAPI.Dogma.Attribute>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/dogma/attributes/{0}/?datasource=tranquility", attribute_id));
        }

        public async Task<Result<int[]>> GetAttributes()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/dogma/attributes/?datasource=tranquility");
        }

        public async Task<Result<Effect>> GetEffect(int effect_id)
        {
            return await base.Execute<Effect>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/dogma/effects/{0}/?datasource=tranquility", effect_id));
        }

        public async Task<Result<int[]>> GetEffects()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v1/dogma/effects/?datasource=tranquility");
        }
    }
}
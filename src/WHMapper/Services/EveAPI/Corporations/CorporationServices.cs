using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Corporation;

namespace WHMapper.Services.EveAPI.Corporations
{
    public class CorporationServices : EveApiServiceBase, ICorporationServices

    {
        public CorporationServices(HttpClient httpClient) 
        : base(httpClient)
        {
        }

        public async Task<Corporation?> GetCorporation(int corporation_id)
        {
            return await Execute<Corporation>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v5/corporations/{0}/?datasource=tranquility", corporation_id));
        }
    }
}
using WHMapper.Shared.Models.DTO.EveAPI.Corporation;

namespace WHMapper.Shared.Services.EveAPI.Corporations
{
    public class CorporationServices : EveApiServiceBase, ICorporationServices

    {
        public CorporationServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Corporation?> GetCorporation(int corporation_id)
        {
            return await Execute<Corporation>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v5/corporations/{0}/?datasource=tranquility", corporation_id));
        }
    }
}
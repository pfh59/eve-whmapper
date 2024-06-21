namespace WHMapper.Services.EveAPI.Corporation
{
    public class CorporationServices : AEveApiServices,ICorporationServices

    {
		public CorporationServices(HttpClient httpClient) : base(httpClient)
        {
		}

        public async Task<Models.DTO.EveAPI.Corporation.Corporation?> GetCorporation(int corporation_id)
        {
            return await base.Execute<Models.DTO.EveAPI.Corporation.Corporation>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v5/corporations/{0}/?datasource=tranquility", corporation_id));

        }

        /*public Task<int[]> GetCorporations()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v2/alliances/?datasource=tranquility");
        }*/
    }
}


namespace WHMapper.Services.EveAPI.Alliances
{
    public class AllianceServices : EveApiServiceBase, IAllianceServices
    {
        public AllianceServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Models.DTO.EveAPI.Alliance.Alliance?> GetAlliance(int alliance_id)
        {
            return await base.Execute<Models.DTO.EveAPI.Alliance.Alliance>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v4/alliances/{0}/?datasource=tranquility", alliance_id));
        }

        public async Task<int[]?> GetAlliances()
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, "/v2/alliances/?datasource=tranquility");
        }
    }
}
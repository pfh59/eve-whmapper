using WHMapper.Shared.Models.DTO.EveAPI.Route.Enums;

namespace WHMapper.Shared.Services.EveAPI.Routes
{
    public class RouteServices : EveApiServiceBase, IRouteServices
    {
        public RouteServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<int[]?> GetRoute(int from, int to)
        {
            return await GetRoute(from, to, null, null);
        }

        public async Task<int[]?> GetRoute(int from, int to, int[]? avoid)
        {
            return await GetRoute(from, to, avoid, null);
        }

        public async Task<int[]?> GetRoute(int from, int to, int[][]? connections)
        {
            return await GetRoute(from, to, null, connections);
        }

        public async Task<int[]?> GetRoute(int from, int to, int[]? avoid, int[][]? connections)
        {
            return await GetRoute(from, to, RouteType.Shortest, avoid, connections);
        }

        public async Task<int[]?> GetRoute(int from, int to, RouteType routeType, int[]? avoid, int[][]? connections)
        {
            if (avoid != null && connections != null)
            {
                return await Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility&avoid={2}&connections={3}&flag={4}", from, to, string.Join(",", avoid), string.Join(",", connections.Select(x => string.Join("|", x))), routeType.ToString().ToLower()));
            }
            else if (avoid != null)
            {
                return await Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility&avoid={2}&flag={3}", from, to, string.Join(",", avoid), routeType.ToString().ToLower()));
            }
            else if (connections != null)
            {
                return await Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility&connections={2}&flag={3}", from, to, string.Join(",", connections.Select(x => string.Join("|", x))), routeType.ToString().ToLower()));
            }

            return await Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility&flag={2}", from, to, routeType.ToString().ToLower()));
        }
    }
}

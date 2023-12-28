using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI;

namespace WHMapper;

public class RouteServices : AEveApiServices,IRouteServices
{
    public RouteServices(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<int[]> GetRoute(int from, int to)
    {
        return await GetRoute(from, to, null,null);
    }

    public async Task<int[]> GetRoute(int from, int to, int[]? avoid)
    {
        return await GetRoute(from, to, avoid,null);
    }

    public async Task<int[]> GetRoute(int from, int to, int[][]? connections)
    {
        return await GetRoute(from, to, null, connections);
    }

    public async Task<int[]> GetRoute(int from, int to, int[]? avoid, int[][]? connections)
    {
        if (avoid != null && connections != null)
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility&avoid={2}&connections={3}", from, to, string.Join(",", avoid), string.Join(",", connections.Select(x=>string.Join("|",x)))));
        }
        else if (avoid != null)
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility&avoid={2}", from, to, string.Join(",", avoid)));
        }
        else if (connections != null)
        {
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility&connections={2}", from, to, string.Join(",", connections.Select(x=>string.Join("|",x)))));
        }
        else
            return await base.Execute<int[]>(RequestSecurity.Public, RequestMethod.Get, string.Format("/v1/route/{0}/{1}/?datasource=tranquility", from, to));

    }

}

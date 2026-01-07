using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Route;
using WHMapper.Models.DTO.EveAPI.Route.Enums;

namespace WHMapper.Services.EveAPI.Routes
{
    public class RouteServices : EveApiServiceBase, IRouteServices
    {
        public RouteServices(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<int[]>> GetRoute(int from, int to)
        {
            return await GetRoute(from, to, null, null);
        }

        public async Task<Result<int[]>> GetRoute(int from, int to, int[]? avoid)
        {
            return await GetRoute(from, to, avoid, null);
        }

        public async Task<Result<int[]>> GetRoute(int from, int to, RouteConnection[]? connections)
        {
            return await GetRoute(from, to, null, connections);
        }

        public async Task<Result<int[]>> GetRoute(int from, int to, int[]? avoid, RouteConnection[]? connections)
        {
            return await GetRoute(from, to, RouteType.Shortest, avoid, connections);
        }

        public async Task<Result<int[]>> GetRoute(int from, int to, RouteType routeType, int[]? avoid, RouteConnection[]? connections)
        {
            var requestBody = new RouteRequest
            {
                AvoidSystems = avoid,
                Connections = connections,
                Preference = MapRouteTypeToPreference(routeType)
            };

            var uri = $"/route/{from}/{to}";
            var result = await Execute<RouteResponse>(RequestSecurity.Public, RequestMethod.Post, uri, requestBody);

            if (result.IsSuccess && result.Data != null)
            {
                return Result<int[]>.Success(result.Data.Route);
            }

            return Result<int[]>.Failure(result.ErrorMessage ?? "Failed to get route");
        }

        private static string MapRouteTypeToPreference(RouteType routeType)
        {
            return routeType switch
            {
                RouteType.Shortest => "Shorter",
                RouteType.Secure => "Safer",
                RouteType.Insecure => "LessSecure",
                _ => "Shorter"
            };
        }
    }
}
using WHMapper.Models.DTO.EveAPI.Route.Enums;

namespace WHMapper;

public interface IRouteServices
{
    Task<int[]> GetRoute(int from, int to);
    Task<int[]> GetRoute(int from, int to, int[]? avoid);
    Task<int[]> GetRoute(int from, int to, int[][]? connections);
    Task<int[]> GetRoute(int from, int to, int[]? avoid, int[][]? connections);
    Task<int[]> GetRoute(int from, int to, RouteType routeType, int[]? avoid, int[][]? connections);
}

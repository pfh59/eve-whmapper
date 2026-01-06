using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Route;
using WHMapper.Models.DTO.EveAPI.Route.Enums;

namespace WHMapper.Services.EveAPI.Routes
{
    public interface IRouteServices
    {
        Task<Result<int[]>> GetRoute(int from, int to);
        Task<Result<int[]>> GetRoute(int from, int to, int[]? avoid);
        Task<Result<int[]>> GetRoute(int from, int to, RouteConnection[]? connections);
        Task<Result<int[]>> GetRoute(int from, int to, int[]? avoid, RouteConnection[]? connections);
        Task<Result<int[]>> GetRoute(int from, int to, RouteType routeType, int[]? avoid, RouteConnection[]? connections);
    }
}
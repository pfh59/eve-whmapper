using WHMapper.Shared.Models.Db;
using WHMapper.Shared.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Shared.Models.DTO.EveMapper;
using WHMapper.Shared.Models.DTO.RoutePlanner;

namespace WHMapper.Shared.Services.EveMapper
{
    public interface IEveMapperRoutePlannerHelper
    {
        /// <summary>
        /// Get all routes from a solar system
        /// mapperConnections is the array of all mapper connection between two Wormhole.
        /// </summary>
        /// <param name="fromSolarSystemId"></param>
        /// <param name="extraConnections"></param>
        /// <returns></returns>
        Task<IEnumerable<EveRoute>?> GetRoutesForAll(int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections);

        /// <summary>
        /// Get my favorites routes from selected system
        /// </summary>
        /// <param name="fromSolarSystemId"></param>
        /// <param name="extraConnections"></param>
        /// <returns></returns>
        Task<IEnumerable<EveRoute>?> GetMyRoutes(int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections);

        Task<WHRoute?> AddRoute(int soloarSystemId, bool global);

        Task<bool> DeleteRoute(int routeId);
    }
}

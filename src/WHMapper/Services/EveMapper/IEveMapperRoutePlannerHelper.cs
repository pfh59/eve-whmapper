using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.RoutePlanner;

namespace WHMapper
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
        Task<IEnumerable<EveRoute>?> GetRoutesForAll(int mapId,int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections);

        /// <summary>
        /// Get my favorites routes from selected system
        /// </summary>
        /// <param name="fromSolarSystemId"></param>
        /// <param name="extraConnections"></param>
        /// <returns></returns>
        Task<IEnumerable<EveRoute>?> GetMyRoutes(int mapId,int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections);


        /// <summary>
        /// Get Thera routes from selected system
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="fromSolarSystemId"></param>
        /// <param name="routeType"></param>
        /// <param name="extraConnections"></param>
        /// <returns></returns>
        Task<IEnumerable<EveRoute>?> GetTheraRoutes(int mapId,int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections);

        /// <summary>
        /// Get Turnur routes from selected system
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="fromSolarSystemId"></param>
        /// <param name="routeType"></param>
        /// <param name="extraConnections"></param>
        /// <returns></returns>
        Task<IEnumerable<EveRoute>?> GetTurnurRoutes(int mapId,int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections);



        Task<WHRoute?> AddRoute(int mapId,int soloarSystemId, bool global);

        Task<bool> DeleteRoute(int routeId);
    }
}

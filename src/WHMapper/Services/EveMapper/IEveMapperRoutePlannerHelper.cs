using System.Collections.Concurrent;
using System.Collections.Frozen;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.RoutePlanner;
namespace WHMapper;


public interface IEveMapperRoutePlannerHelper
{
    /// <summary>
    /// Get all routes from a solar system
    /// mapperConnections is the array of all mapper connection between two Wormhole.
    /// </summary>
    /// <param name="fromSolarSystemId"></param>
    /// <param name="extraConnections"></param>
    /// <returns></returns>
    Task<IEnumerable<EveRoute>?> GetRoutesForAll(int fromSolarSystemId,RouteType routeType,IEnumerable<RouteConnection>? extraConnections);

    /// <summary>
    /// Get my favorites routes from selected system
    /// </summary>
    /// <param name="fromSolarSystemId"></param>
    /// <param name="extraConnections"></param>
    /// <returns></returns>
    Task<IEnumerable<EveRoute>?> GetMyRoutes(int fromSolarSystemId,RouteType routeType,IEnumerable<RouteConnection>? extraConnections);
    
    Task<WHRoute> AddRoute(int soloarSystemId,bool global);
   
    Task<bool> DeleteRoute(int routeId);
}

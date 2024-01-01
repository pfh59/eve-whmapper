namespace WHMapper;


public interface IEveMapperRoutePlannerHelper
{
    /// <summary>
    /// Get all routes from a solar system
    /// mapperConnections is the array of all mapper connection between two Wormhole.
    /// </summary>
    /// <param name="fromSolarSystemId"></param>
    /// <param name="mapperConnections"></param>
    /// <returns></returns>
    Task<IEnumerable<EveRoute>?> GetRoutesForAll(int fromSolarSystemId,int[][]? mapperConnections);

    /// <summary>
    /// Get my favorites routes from selected system
    /// </summary>
    /// <param name="fromSolarSystemId"></param>
    /// <param name="mapperConnections"></param>
    /// <returns></returns>
    Task<IEnumerable<EveRoute>?> GetMyRoutes(int fromSolarSystemId,int[][]? mapperConnections);
    
    Task<WHRoute> AddRoute(int soloarSystemId,bool global);
   
    Task<bool> DeleteRoute(int routeId);
}

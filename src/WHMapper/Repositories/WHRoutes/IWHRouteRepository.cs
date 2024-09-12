using WHMapper.Repositories;

namespace WHMapper;

public interface IWHRouteRepository : IDefaultRepository<WHRoute, int>
{
    Task<IEnumerable<WHRoute>> GetRoutesByEveEntityId(int mapId,int eveEntityId);
    Task<IEnumerable<WHRoute>> GetRoutesForAll(int mapId);

}

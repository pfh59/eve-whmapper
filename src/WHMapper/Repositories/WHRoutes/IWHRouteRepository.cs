using WHMapper.Repositories;

namespace WHMapper;

public interface IWHRouteRepository : IDefaultRepository<WHRoute, int>
{
    Task<IEnumerable<WHRoute>> GetRoutesByEveEntityId(int eveEntityId);
    Task<IEnumerable<WHRoute>> GetRoutesForAll();

}

using WHMapper.Shared.Models.Db;

namespace WHMapper.Shared.Repositories.WHRoutes;

public interface IWHRouteRepository : IDefaultRepository<WHRoute, int>
{
    Task<IEnumerable<WHRoute>> GetRoutesByEveEntityId(int eveEntityId);
    Task<IEnumerable<WHRoute>> GetRoutesForAll();

}

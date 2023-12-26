namespace WHMapper;

public interface IEveMapperRoutePlannerHelper
{
    public Task<IEnumerable<WHRoute>> GetRoutesForAll();
    public Task<IEnumerable<WHRoute>> GetMyRoutes();
    
}


using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper;

public class EveMapperRoutePlannerHelper : IEveMapperRoutePlannerHelper
{
    private readonly IWHRouteRepository _routeRepository;
    private readonly ILogger<EveMapperRoutePlannerHelper> _logger;
    private readonly IEveUserInfosServices _eveUserInfosServices;

    public EveMapperRoutePlannerHelper(ILogger<EveMapperRoutePlannerHelper> logger,IWHRouteRepository routeRepository, IEveUserInfosServices eveUserInfosServices)
    {
        _routeRepository = routeRepository;
        _logger = logger;
        _eveUserInfosServices = eveUserInfosServices;
    }

    public async Task<IEnumerable<WHRoute>> GetMyRoutes()
    {
        if(_routeRepository == null)
        {
            _logger.LogError("Route Repository is null");
            return null;
        }


        if(_eveUserInfosServices == null)
        {
            _logger.LogError("EveUserInfosServices is null");
            return null;
        }

        var characterId = await _eveUserInfosServices.GetCharactedID();
        return await _routeRepository.GetRoutesByEveEntityId(characterId);    
    }

    public Task<IEnumerable<WHRoute>> GetRoutesForAll()
    {
        if(_routeRepository == null)
        {
            _logger.LogError("Route Repository is null");
            return null;
        }

        return _routeRepository.GetRoutesForAll();
    }
}

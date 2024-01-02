
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper;

public class EveMapperRoutePlannerHelper : IEveMapperRoutePlannerHelper
{
    private readonly IWHRouteRepository _routeRepository;
    private readonly ILogger<EveMapperRoutePlannerHelper> _logger;
    private readonly IEveUserInfosServices _eveUserInfosServices;

    private readonly IEveAPIServices _eveAPIService;

    public EveMapperRoutePlannerHelper(ILogger<EveMapperRoutePlannerHelper> logger,IWHRouteRepository routeRepository, IEveUserInfosServices eveUserInfosServices, IEveAPIServices eveAPIService)
    {
        _routeRepository = routeRepository;
        _logger = logger;
        _eveUserInfosServices = eveUserInfosServices;
        _eveAPIService = eveAPIService;
    }

    public async Task<IEnumerable<EveRoute>?> GetMyRoutes(int fromSolarSystemId,RouteType routeType,int[][]? extraConnections=null)
    {
        return await GetRoutes(fromSolarSystemId,routeType,extraConnections,false);
    }

    public async Task<IEnumerable<EveRoute>?> GetRoutesForAll(int fromSolarSystemId,RouteType routeType,int[][]? extraConnections)
    {
        return await GetRoutes(fromSolarSystemId,routeType,extraConnections,true);
    }

    private async Task<IEnumerable<EveRoute>?> GetRoutes(int fromSolarSystemId,RouteType routeType,int[][]? extraConnections,bool global)
    {
        IList<EveRoute> routes = new List<EveRoute>();
        IEnumerable<WHRoute> whRoutes;

        if(_routeRepository == null)
        {
            _logger.LogError("Route Repository is null");
            return null;
        }

        if(global)
            whRoutes = await _routeRepository.GetRoutesForAll();
        else
        {
            if(_eveUserInfosServices == null)
            {
                _logger.LogError("EveUserInfosServices is null");
                return null;
            }
            var characterId = await _eveUserInfosServices.GetCharactedID();
            whRoutes = await _routeRepository.GetRoutesByEveEntityId(characterId);
        }   


        foreach (var whRoute in whRoutes)
        {
            var destSystemInfo = await _eveAPIService.UniverseServices.GetSystem(whRoute.SolarSystemId);
            var route = await _eveAPIService.RouteServices.GetRoute(fromSolarSystemId,whRoute.SolarSystemId,routeType,null,extraConnections);
            routes.Add(new EveRoute(whRoute.Id,destSystemInfo.Name,route));
        }
        return routes;
    }



    public async Task<WHRoute> AddRoute(int soloarSystemId,bool global)
    {
        WHRoute? route = null;
                
        if(global)
        {
            route = await _routeRepository.Create(new WHRoute(soloarSystemId));
        }
        else
        {
            if(_eveUserInfosServices == null)
            {
                _logger.LogError("EveUserInfosServices is null");
                return null;
            }
            var characterId = await _eveUserInfosServices.GetCharactedID();
            route = await _routeRepository.Create(new WHRoute(soloarSystemId,characterId));
        }

        if(route == null)
        {
                _logger.LogError("Error creating route");
                return null;
        }
        return route;
    }

    public Task<bool> DeleteRoute(int routeId)
    {
        return _routeRepository.DeleteById(routeId);
    }
}


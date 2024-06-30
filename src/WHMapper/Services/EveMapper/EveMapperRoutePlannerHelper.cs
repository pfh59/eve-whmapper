﻿using FibonacciHeap;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.RoutePlanner;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.EveOnlineUserInfosProvider;
using WHMapper.Services.SDE;

namespace WHMapper;

public class EveMapperRoutePlannerHelper : IEveMapperRoutePlannerHelper
{
    private readonly IWHRouteRepository _routeRepository;
    private readonly ILogger<EveMapperRoutePlannerHelper> _logger;
    private readonly IEveUserInfosServices _eveUserInfosServices;
    private readonly ISDEService _sdeServices;
    private IEnumerable<SolarSystemJump> _solarSystemJumpConnections = null!;

    public EveMapperRoutePlannerHelper(ILogger<EveMapperRoutePlannerHelper> logger, IWHRouteRepository routeRepository, IEveUserInfosServices eveUserInfosServices, ISDEService sdeServices)
    {
        _routeRepository = routeRepository;
        _logger = logger;
        _eveUserInfosServices = eveUserInfosServices;
        _sdeServices = sdeServices;
    }

    public async Task<IEnumerable<EveRoute>?> GetMyRoutes(int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections = null)
    {
        if (_eveUserInfosServices == null)
        {
            _logger.LogError("EveUserInfosServices is null");
            return null;
        }

        return await GetRoutes(fromSolarSystemId, routeType, extraConnections, false);
    }

    public async Task<IEnumerable<EveRoute>?> GetRoutesForAll(int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections = null)
    {
        if (_routeRepository == null)
        {
            _logger.LogError("Route Repository is null");
            return null;
        }

        return await GetRoutes(fromSolarSystemId, routeType, extraConnections, true);
    }

    private async Task<IEnumerable<EveRoute>?> GetRoutes(int fromSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections, bool global)
    {
        IList<EveRoute> routes = new List<EveRoute>();
        IEnumerable<WHRoute> whRoutes;

        if (global)
        {
            whRoutes = await _routeRepository.GetRoutesForAll();
        }
        else
        {
            var characterId = await _eveUserInfosServices.GetCharactedID();
            whRoutes = await _routeRepository.GetRoutesByEveEntityId(characterId);
        }

        foreach (var whRoute in whRoutes)
        {
            var destSystemInfo = await _sdeServices.SearchSystemById(whRoute.SolarSystemId);
            string destName = (destSystemInfo != null) ? destSystemInfo.Name : string.Empty;
            var route = await CalculateRoute(fromSolarSystemId, whRoute.SolarSystemId, routeType, extraConnections);
            routes.Add(new EveRoute(whRoute.Id, destName, route));
        }

        return routes;
    }

    /// <summary>
    /// Theory : https://www.cs.princeton.edu/courses/archive/spring13/cos423/lectures/FibonacciHeaps-2x2.pdf
    /// CCP esi-route : https://github.com/esi/esi-routes/blob/master/esi_routes/dijkstra.py
    /// </summary>
    /// <param name="fromSolarSystemId"></param>
    /// <param name="toSolarSystemId"></param>
    /// <param name="routeType"></param>
    /// <param name="extraConnections"></param>
    /// <returns></returns>
    private async Task<int[]?> CalculateRoute(int fromSolarSystemId, int toSolarSystemId, RouteType routeType, IEnumerable<RouteConnection>? extraConnections)
    {
        if (_solarSystemJumpConnections == null)
        {
            var solarSystemJumpList = await _sdeServices.GetSolarSystemJumpList();
            _solarSystemJumpConnections = solarSystemJumpList != null ? solarSystemJumpList.ToArray() : new SolarSystemJump[0];
        }

        IEnumerable<SolarSystemJump> extendedSolarSystemJumps = _solarSystemJumpConnections.Select(x => new SolarSystemJump(x.System.SolarSystemId, x.System.Security, x.JumpList.ToArray())).ToArray();

        if (extraConnections != null)
        {
            foreach (var extraConnection in extraConnections)
            {
                var solarSystem = extendedSolarSystemJumps.FirstOrDefault(x => x.System.SolarSystemId == extraConnection.FromSolarSystemId);

                if (solarSystem == null)
                {
                    extendedSolarSystemJumps = extendedSolarSystemJumps.Append(new SolarSystemJump(extraConnection.FromSolarSystemId, extraConnection.FromSecurity, new List<SolarSystem>() { new SolarSystem(extraConnection.ToSolarSystemId, extraConnection.ToSecurity) }));
                }
                else
                {
                    if (solarSystem.JumpList != null && solarSystem.JumpList?.FirstOrDefault(x => x.SolarSystemId == toSolarSystemId) == null)
                    {
                        solarSystem.JumpList = solarSystem!.JumpList!.Append(new SolarSystem(extraConnection.ToSolarSystemId, extraConnection.ToSecurity));
                    }
                }
            }
        }

        var fromSolarSystemJump = extendedSolarSystemJumps.FirstOrDefault(x => x.System.SolarSystemId == fromSolarSystemId);
        var toSolarSystemJump = extendedSolarSystemJumps.FirstOrDefault(x => x.System.SolarSystemId == toSolarSystemId);

        if (fromSolarSystemJump == null || toSolarSystemJump == null)
        {
            _logger.LogError("Solar System {0} doesn't exist", fromSolarSystemId);
            return null;
        }

        if (fromSolarSystemJump.JumpList.Count() == 0)
        {
            _logger.LogWarning("No connection from solar system {0}", fromSolarSystemId);
            return new int[0];
        }

        if (fromSolarSystemJump.JumpList?.FirstOrDefault(x => x.SolarSystemId == toSolarSystemId) != null)
        {
            return new int[2] { fromSolarSystemId, toSolarSystemId };
        }


        IList<int> routePath = new List<int>();
        IList<int> remainingSystems = new List<int>();
        IDictionary<int, FibonacciHeapNode<int, float>> entries = new Dictionary<int, FibonacciHeapNode<int, float>>();
        IDictionary<int, int> previousSystems = new Dictionary<int, int>();
        IDictionary<int, float> costs = new Dictionary<int, float>();

        FibonacciHeap<int, float> queue = new FibonacciHeap<int, float>(0);

        remainingSystems.Add(toSolarSystemId);

        costs.Add(fromSolarSystemId, 0.0f);
        entries.Add(fromSolarSystemId, new FibonacciHeapNode<int, float>(fromSolarSystemId, 0.0f));
        queue.Insert(entries[fromSolarSystemId]);

        int currentSystem;
        while (!queue.IsEmpty())
        {
            currentSystem = queue.RemoveMin().Data;

            if (remainingSystems.Contains(currentSystem))
                remainingSystems.Remove(currentSystem);

            //all found
            if (remainingSystems.Count == 0)
                break;

            IEnumerable<SolarSystem>? nextSystems = extendedSolarSystemJumps.Where(x => x.System.SolarSystemId == currentSystem && x.JumpList != null).SelectMany(x => x.JumpList).ToArray();

            foreach (SolarSystem nextSystem in nextSystems)
            {
                //case of wh with no link on map
                if (nextSystem == null)
                    continue;

                //system already view
                if (previousSystems.ContainsKey(nextSystem.SolarSystemId))
                    continue;


                var newCost = costs[currentSystem] + getCost(routeType, nextSystem.Security);
                if (costs.ContainsKey(nextSystem.SolarSystemId) && newCost < costs[nextSystem.SolarSystemId])
                {
                    costs[nextSystem.SolarSystemId] = newCost;
                    previousSystems.Add(nextSystem.SolarSystemId, currentSystem);
                    queue.DecreaseKey(entries[nextSystem.SolarSystemId], costs[nextSystem.SolarSystemId]);
                }

                if (!costs.ContainsKey(nextSystem.SolarSystemId))
                {
                    costs.Add(nextSystem.SolarSystemId, newCost);
                    previousSystems.Add(nextSystem.SolarSystemId, currentSystem);
                    var node = new FibonacciHeapNode<int, float>(nextSystem.SolarSystemId, costs[nextSystem.SolarSystemId]);
                    entries.Add(nextSystem.SolarSystemId, node);
                    queue.Insert(entries[nextSystem.SolarSystemId]);
                }
            }
        }

        //all path found, return route Path array
        int tmpSystem = toSolarSystemId;
        while (tmpSystem != fromSolarSystemId)
        {
            routePath.Add(tmpSystem);
            if (!previousSystems.ContainsKey(tmpSystem))
            {
                _logger.LogError("Error calculating route");
                return null;
            }

            tmpSystem = previousSystems[tmpSystem];
        }
        routePath.Add(fromSolarSystemId);

        return routePath.Reverse().ToArray();
    }

    private float getCost(RouteType type, float security)
    {
        switch (type)
        {
            case RouteType.Shortest:
                return 1.0f;
            case RouteType.Secure:
                if (security < 0.45f)
                    return 50000.0f;

                return 1.0f;
            case RouteType.Insecure:
                if (security >= 0.45f)
                    return 50000.0f;

                return 1.0f;
            default:
                return 1.0f;
        }
    }

    public async Task<WHRoute?> AddRoute(int soloarSystemId, bool global)
    {
        WHRoute? route = null;

        if (global)
        {
            route = await _routeRepository.Create(new WHRoute(soloarSystemId));
        }
        else
        {
            if (_eveUserInfosServices == null)
            {
                _logger.LogError("EveUserInfosServices is null");
                return null;
            }
            var characterId = await _eveUserInfosServices.GetCharactedID();
            route = await _routeRepository.Create(new WHRoute(soloarSystemId, characterId));
        }

        if (route == null)
        {
            _logger.LogInformation("No route available");
            return null;
        }
        return route;
    }

    public Task<bool> DeleteRoute(int routeId)
    {
        return _routeRepository.DeleteById(routeId);
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Route.Enums;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO.EveScout;
using WHMapper.Models.DTO.RoutePlanner;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveScoutAPI;
using WHMapper.Services.SDE;

namespace WHMapper.Tests.WHHelper;

public class EveWHMapperRoutePlannerHelperTest
{
    private const string CLIENT_ID = "test-client-id";
    private const int MAP_ID = 42;
    private const int PRIMARY_CHARACTER_ID = 999;

    // Main topology (linear chain) for basic CalculateRoute coverage
    private const int S1 = 1;   // sec 1.0
    private const int S2 = 2;   // sec 0.5
    private const int S3 = 3;   // sec 0.3
    private const int S4 = 4;   // sec 0.1
    private const int S5 = 5;   // sec 0.5 - isolated
    private const int S6 = 6;   // sec 0.4 - not in SDE, used via extraConnections

    // Diamond topology for security-routing tests
    private const int H100 = 100; // sec 1.0
    private const int H101 = 101; // sec 0.5
    private const int H102 = 102; // sec 0.6
    private const int H103 = 103; // sec 0.3
    private const int H104 = 104; // sec 0.3 (destination)

    private readonly Mock<IWHRouteRepository> _routeRepoMock;
    private readonly Mock<IEveMapperUserManagementService> _userMgmtMock;
    private readonly Mock<ISDEService> _sdeMock;
    private readonly Mock<IEveScoutAPIServices> _eveScoutMock;
    private readonly ClientUID _clientUid;
    private readonly ILogger<EveMapperRoutePlannerHelper> _logger;

    public EveWHMapperRoutePlannerHelperTest()
    {
        _routeRepoMock = new Mock<IWHRouteRepository>();
        _userMgmtMock = new Mock<IEveMapperUserManagementService>();
        _sdeMock = new Mock<ISDEService>();
        _eveScoutMock = new Mock<IEveScoutAPIServices>();
        _clientUid = new ClientUID { ClientId = CLIENT_ID };
        _logger = new NullLogger<EveMapperRoutePlannerHelper>();

        _sdeMock.Setup(s => s.GetSolarSystemJumpList()).ReturnsAsync(BuildLinearJumpList());
        _sdeMock.Setup(s => s.SearchSystemById(It.IsAny<int>()))
            .ReturnsAsync((int id) => new SDESolarSystem { SolarSystemID = id, Name = $"S{id}" });

        _userMgmtMock.Setup(u => u.GetPrimaryAccountAsync(CLIENT_ID))
            .ReturnsAsync(new WHMapperUser(PRIMARY_CHARACTER_ID, "portrait.jpg"));
    }

    private EveMapperRoutePlannerHelper CreateHelper(
        IWHRouteRepository? routeRepo = null,
        IEveMapperUserManagementService? userMgmt = null,
        ClientUID? clientUid = null,
        ISDEService? sdeService = null,
        IEveScoutAPIServices? eveScout = null)
    {
        return new EveMapperRoutePlannerHelper(
            _logger,
            routeRepo ?? _routeRepoMock.Object,
            userMgmt ?? _userMgmtMock.Object,
            clientUid ?? _clientUid,
            sdeService ?? _sdeMock.Object,
            eveScout ?? _eveScoutMock.Object);
    }

    private static List<SolarSystemJump> BuildLinearJumpList()
    {
        return new List<SolarSystemJump>
        {
            new SolarSystemJump(S1, 1.0f, new[] { new SolarSystem(S2, 0.5f) }),
            new SolarSystemJump(S2, 0.5f, new[] { new SolarSystem(S1, 1.0f), new SolarSystem(S3, 0.3f) }),
            new SolarSystemJump(S3, 0.3f, new[] { new SolarSystem(S2, 0.5f), new SolarSystem(S4, 0.1f) }),
            new SolarSystemJump(S4, 0.1f, new[] { new SolarSystem(S3, 0.3f) }),
            new SolarSystemJump(S5, 0.5f, Array.Empty<SolarSystem>())
        };
    }

    private static List<SolarSystemJump> BuildDiamondJumpList()
    {
        return new List<SolarSystemJump>
        {
            new SolarSystemJump(H100, 1.0f, new[] { new SolarSystem(H101, 0.5f), new SolarSystem(H103, 0.3f) }),
            new SolarSystemJump(H101, 0.5f, new[] { new SolarSystem(H100, 1.0f), new SolarSystem(H102, 0.6f) }),
            new SolarSystemJump(H102, 0.6f, new[] { new SolarSystem(H101, 0.5f), new SolarSystem(H104, 0.3f) }),
            new SolarSystemJump(H103, 0.3f, new[] { new SolarSystem(H100, 1.0f), new SolarSystem(H104, 0.3f) }),
            new SolarSystemJump(H104, 0.3f, new[] { new SolarSystem(H102, 0.6f), new SolarSystem(H103, 0.3f) })
        };
    }

    // ===========================================================
    // AddRoute
    // ===========================================================

    [Fact]
    public async Task AddRoute_Global_True_CallsCreateWithoutEntityId()
    {
        var created = new WHRoute(MAP_ID, S1) { Id = 7 };
        _routeRepoMock.Setup(r => r.Create(It.IsAny<WHRoute>())).ReturnsAsync(created);

        var helper = CreateHelper();
        var result = await helper.AddRoute(MAP_ID, S1, global: true);

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        _routeRepoMock.Verify(r => r.Create(It.Is<WHRoute>(x =>
            x.MapId == MAP_ID && x.SolarSystemId == S1 && x.EveEntityId == null)), Times.Once);
    }

    [Fact]
    public async Task AddRoute_Global_False_WithPrimaryAccount_CallsCreateWithEntityId()
    {
        var created = new WHRoute(MAP_ID, S1, PRIMARY_CHARACTER_ID) { Id = 8 };
        _routeRepoMock.Setup(r => r.Create(It.IsAny<WHRoute>())).ReturnsAsync(created);

        var helper = CreateHelper();
        var result = await helper.AddRoute(MAP_ID, S1, global: false);

        Assert.NotNull(result);
        Assert.Equal(8, result!.Id);
        _routeRepoMock.Verify(r => r.Create(It.Is<WHRoute>(x =>
            x.MapId == MAP_ID && x.SolarSystemId == S1 && x.EveEntityId == PRIMARY_CHARACTER_ID)), Times.Once);
    }

    [Fact]
    public async Task AddRoute_Global_False_NoClientId_ReturnsNull()
    {
        var clientWithNoId = new ClientUID { ClientId = null };
        var helper = CreateHelper(clientUid: clientWithNoId);

        var result = await helper.AddRoute(MAP_ID, S1, global: false);

        Assert.Null(result);
        _routeRepoMock.Verify(r => r.Create(It.IsAny<WHRoute>()), Times.Never);
    }

    [Fact]
    public async Task AddRoute_Global_False_NoPrimaryAccount_ReturnsNull()
    {
        _userMgmtMock.Setup(u => u.GetPrimaryAccountAsync(CLIENT_ID)).ReturnsAsync((WHMapperUser?)null);

        var helper = CreateHelper();
        var result = await helper.AddRoute(MAP_ID, S1, global: false);

        Assert.Null(result);
        _routeRepoMock.Verify(r => r.Create(It.IsAny<WHRoute>()), Times.Never);
    }

    [Fact]
    public async Task AddRoute_RepositoryReturnsNull_ReturnsNull()
    {
        _routeRepoMock.Setup(r => r.Create(It.IsAny<WHRoute>())).ReturnsAsync((WHRoute?)null);

        var helper = CreateHelper();
        var result = await helper.AddRoute(MAP_ID, S1, global: true);

        Assert.Null(result);
    }

    // ===========================================================
    // DeleteRoute
    // ===========================================================

    [Fact]
    public async Task DeleteRoute_ReturnsTrue_WhenRepositoryReturnsTrue()
    {
        _routeRepoMock.Setup(r => r.DeleteById(123)).ReturnsAsync(true);

        var helper = CreateHelper();
        var result = await helper.DeleteRoute(123);

        Assert.True(result);
        _routeRepoMock.Verify(r => r.DeleteById(123), Times.Once);
    }

    [Fact]
    public async Task DeleteRoute_ReturnsFalse_WhenRepositoryReturnsFalse()
    {
        _routeRepoMock.Setup(r => r.DeleteById(456)).ReturnsAsync(false);

        var helper = CreateHelper();
        var result = await helper.DeleteRoute(456);

        Assert.False(result);
    }

    // ===========================================================
    // GetMyRoutes
    // ===========================================================

    [Fact]
    public async Task GetMyRoutes_NoClientId_ReturnsNull()
    {
        var clientWithNoId = new ClientUID { ClientId = null };
        var helper = CreateHelper(clientUid: clientWithNoId);

        var result = await helper.GetMyRoutes(MAP_ID, S1, RouteType.Shortest, null);

        Assert.Null(result);
        _routeRepoMock.Verify(r => r.GetRoutesByEveEntityId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetMyRoutes_NoPrimaryAccount_ReturnsNull()
    {
        _userMgmtMock.Setup(u => u.GetPrimaryAccountAsync(CLIENT_ID)).ReturnsAsync((WHMapperUser?)null);

        var helper = CreateHelper();
        var result = await helper.GetMyRoutes(MAP_ID, S1, RouteType.Shortest, null);

        Assert.Null(result);
        _routeRepoMock.Verify(r => r.GetRoutesByEveEntityId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetMyRoutes_WithPrimaryAccount_ReturnsRoutesFromGetRoutesByEveEntityId()
    {
        var whRoutes = new[]
        {
            new WHRoute(MAP_ID, S2, PRIMARY_CHARACTER_ID) { Id = 1 },
            new WHRoute(MAP_ID, S4, PRIMARY_CHARACTER_ID) { Id = 2 }
        };
        _routeRepoMock.Setup(r => r.GetRoutesByEveEntityId(MAP_ID, PRIMARY_CHARACTER_ID))
            .ReturnsAsync(whRoutes);

        var helper = CreateHelper();
        var result = await helper.GetMyRoutes(MAP_ID, S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var routes = result!.ToArray();
        Assert.Equal(2, routes.Length);
        Assert.Equal("S2", routes[0].DestinationName);
        Assert.Equal("S4", routes[1].DestinationName);
        _routeRepoMock.Verify(r => r.GetRoutesByEveEntityId(MAP_ID, PRIMARY_CHARACTER_ID), Times.Once);
        _routeRepoMock.Verify(r => r.GetRoutesForAll(It.IsAny<int>()), Times.Never);
    }

    // ===========================================================
    // GetRoutesForAll
    // ===========================================================

    [Fact]
    public async Task GetRoutesForAll_RepositoryNull_ReturnsNull()
    {
        var helper = new EveMapperRoutePlannerHelper(
            _logger,
            null!,
            _userMgmtMock.Object,
            _clientUid,
            _sdeMock.Object,
            _eveScoutMock.Object);

        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRoutesForAll_ReturnsRoutesFromGetRoutesForAll()
    {
        var whRoutes = new[]
        {
            new WHRoute(MAP_ID, S4) { Id = 11 }
        };
        _routeRepoMock.Setup(r => r.GetRoutesForAll(MAP_ID)).ReturnsAsync(whRoutes);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var routes = result!.ToArray();
        Assert.Single(routes);
        Assert.Equal("S4", routes[0].DestinationName);
        Assert.Equal(11, routes[0].Id);
        Assert.True(routes[0].IsAvailable);
        _routeRepoMock.Verify(r => r.GetRoutesForAll(MAP_ID), Times.Once);
    }

    [Fact]
    public async Task GetRoutesForAll_NoWHRoutes_ReturnsEmpty()
    {
        _routeRepoMock.Setup(r => r.GetRoutesForAll(MAP_ID)).ReturnsAsync(Array.Empty<WHRoute>());

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetRoutesForAll_DestinationNotInSDE_RouteNameIsEmpty()
    {
        var whRoutes = new[] { new WHRoute(MAP_ID, S2) { Id = 1 } };
        _routeRepoMock.Setup(r => r.GetRoutesForAll(MAP_ID)).ReturnsAsync(whRoutes);
        _sdeMock.Setup(s => s.SearchSystemById(S2)).ReturnsAsync((SDESolarSystem?)null);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var route = result!.First();
        Assert.Equal(string.Empty, route.DestinationName);
    }

    // ===========================================================
    // GetTheraRoutes / GetTurnurRoutes
    // ===========================================================

    [Fact]
    public async Task GetTheraRoutes_NoEntries_ReturnsEmpty()
    {
        _eveScoutMock.Setup(e => e.GetTheraSystemsAsync()).ReturnsAsync((IEnumerable<EveScoutSystemEntry>?)null);

        var helper = CreateHelper();
        var result = await helper.GetTheraRoutes(S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetTheraRoutes_EmptyEntries_ReturnsEmpty()
    {
        _eveScoutMock.Setup(e => e.GetTheraSystemsAsync()).ReturnsAsync(Array.Empty<EveScoutSystemEntry>());

        var helper = CreateHelper();
        var result = await helper.GetTheraRoutes(S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetTheraRoutes_SkipsEntriesWithNullInSystemId()
    {
        var entries = new[]
        {
            new EveScoutSystemEntry { InSystemId = null, InSystemName = "Thera" },
            new EveScoutSystemEntry { InSystemId = S2, InSystemName = "S2-Thera" }
        };
        _eveScoutMock.Setup(e => e.GetTheraSystemsAsync()).ReturnsAsync(entries);

        var helper = CreateHelper();
        var result = await helper.GetTheraRoutes(S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var routes = result!.ToArray();
        Assert.Single(routes);
        Assert.Equal("S2-Thera", routes[0].DestinationName);
    }

    [Fact]
    public async Task GetTheraRoutes_SkipsEntriesWithEmptyInSystemName()
    {
        var entries = new[]
        {
            new EveScoutSystemEntry { InSystemId = S2, InSystemName = string.Empty },
            new EveScoutSystemEntry { InSystemId = S4, InSystemName = "S4-Thera" }
        };
        _eveScoutMock.Setup(e => e.GetTheraSystemsAsync()).ReturnsAsync(entries);

        var helper = CreateHelper();
        var result = await helper.GetTheraRoutes(S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var routes = result!.ToArray();
        Assert.Single(routes);
        Assert.Equal("S4-Thera", routes[0].DestinationName);
    }

    [Fact]
    public async Task GetTheraRoutes_BuildsRoutesForValidEntries()
    {
        var entries = new[]
        {
            new EveScoutSystemEntry { InSystemId = S2, InSystemName = "S2-Thera" },
            new EveScoutSystemEntry { InSystemId = S4, InSystemName = "S4-Thera" }
        };
        _eveScoutMock.Setup(e => e.GetTheraSystemsAsync()).ReturnsAsync(entries);

        var helper = CreateHelper();
        var result = await helper.GetTheraRoutes(S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var routes = result!.ToArray();
        Assert.Equal(2, routes.Length);
        Assert.All(routes, r => Assert.Equal(-1, r.Id));
        Assert.Equal("S2-Thera", routes[0].DestinationName);
        Assert.Equal("S4-Thera", routes[1].DestinationName);
        Assert.True(routes[0].IsAvailable);
        Assert.True(routes[1].IsAvailable);
        _eveScoutMock.Verify(e => e.GetTheraSystemsAsync(), Times.Once);
        _eveScoutMock.Verify(e => e.GetTurnurSystemsAsync(), Times.Never);
    }

    [Fact]
    public async Task GetTurnurRoutes_DelegatesToGetTurnurSystemsAsync()
    {
        var entries = new[]
        {
            new EveScoutSystemEntry { InSystemId = S2, InSystemName = "S2-Turnur" }
        };
        _eveScoutMock.Setup(e => e.GetTurnurSystemsAsync()).ReturnsAsync(entries);

        var helper = CreateHelper();
        var result = await helper.GetTurnurRoutes(S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        Assert.Single(result!);
        _eveScoutMock.Verify(e => e.GetTurnurSystemsAsync(), Times.Once);
        _eveScoutMock.Verify(e => e.GetTheraSystemsAsync(), Times.Never);
    }

    // ===========================================================
    // CalculateRoute (via GetRoutesForAll)
    // ===========================================================

    private void SetupSingleDestination(int destinationSystemId)
    {
        _routeRepoMock.Setup(r => r.GetRoutesForAll(MAP_ID))
            .ReturnsAsync(new[] { new WHRoute(MAP_ID, destinationSystemId) { Id = 1 } });
    }

    [Fact]
    public async Task CalculateRoute_FromSystemNotInSDE_RouteIsNull()
    {
        SetupSingleDestination(S2);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, 999999, RouteType.Shortest, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.Null(route.Route);
        Assert.False(route.IsAvailable);
    }

    [Fact]
    public async Task CalculateRoute_ToSystemNotInSDE_RouteIsNull()
    {
        SetupSingleDestination(S6); // S6 is not in the SDE jump list

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.Null(route.Route);
        Assert.False(route.IsAvailable);
    }

    [Fact]
    public async Task CalculateRoute_FromSystemHasEmptyJumpList_ReturnsEmptyArray()
    {
        SetupSingleDestination(S1);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S5, RouteType.Shortest, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        Assert.Empty(route.Route!);
        Assert.Equal(0, route.RouteLength);
        Assert.Equal(0, route.JumpLength);
    }

    [Fact]
    public async Task CalculateRoute_DirectNeighbor_ReturnsTwoSystemArray()
    {
        SetupSingleDestination(S2);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        Assert.Equal(new[] { S1, S2 }, route.Route);
        Assert.Equal(2, route.RouteLength);
        Assert.Equal(1, route.JumpLength);
    }

    [Fact]
    public async Task CalculateRoute_MultiHop_ReturnsShortestPath()
    {
        SetupSingleDestination(S4);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        Assert.Equal(new[] { S1, S2, S3, S4 }, route.Route);
        Assert.Equal(4, route.RouteLength);
        Assert.Equal(3, route.JumpLength);
    }

    [Fact]
    public async Task CalculateRoute_WithExtraConnection_NewSystem_AppendsToJumpList()
    {
        // S6 is NOT in SDE; declare a wormhole from S6 to S4 via extraConnections
        SetupSingleDestination(S4);
        var extraConnections = new[]
        {
            new RouteConnection(S6, -1.0f, S4, 0.1f)
        };

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S6, RouteType.Shortest, extraConnections);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        Assert.Equal(new[] { S6, S4 }, route.Route);
        Assert.Equal(2, route.RouteLength);
    }

    [Fact]
    public async Task CalculateRoute_WithExtraConnection_ExistingSystem_ExtendsJumpList()
    {
        // S2 is in SDE. Add a direct wormhole S2 -> S4, bypassing S3.
        SetupSingleDestination(S4);
        var extraConnections = new[]
        {
            new RouteConnection(S2, 0.5f, S4, 0.1f)
        };

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, RouteType.Shortest, extraConnections);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        // Shortest path is now S1 -> S2 -> S4 thanks to the new connection
        Assert.Equal(new[] { S1, S2, S4 }, route.Route);
        Assert.Equal(3, route.RouteLength);
    }

    [Fact]
    public async Task CalculateRoute_RouteTypeSecure_AvoidsLowSec()
    {
        // Swap SDE topology for the diamond graph
        _sdeMock.Setup(s => s.GetSolarSystemJumpList()).ReturnsAsync(BuildDiamondJumpList());
        SetupSingleDestination(H104);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, H100, RouteType.Secure, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        // Secure routing prefers the high-sec path H100 -> H101 -> H102 -> H104 (length 4)
        Assert.Equal(new[] { H100, H101, H102, H104 }, route.Route);
        Assert.Equal(4, route.RouteLength);
        Assert.DoesNotContain(H103, route.Route!);
    }

    [Fact]
    public async Task CalculateRoute_RouteTypeInsecure_AvoidsHighSec()
    {
        _sdeMock.Setup(s => s.GetSolarSystemJumpList()).ReturnsAsync(BuildDiamondJumpList());
        SetupSingleDestination(H104);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, H100, RouteType.Insecure, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        // Insecure routing prefers the low-sec path H100 -> H103 -> H104 (length 3)
        Assert.Equal(new[] { H100, H103, H104 }, route.Route);
        Assert.Equal(3, route.RouteLength);
        Assert.DoesNotContain(H101, route.Route!);
    }

    [Fact]
    public async Task CalculateRoute_RouteTypeDefault_UsesDefaultCost()
    {
        // Cast an undefined enum value to exercise the `default` branch of getCost
        SetupSingleDestination(S4);

        var helper = CreateHelper();
        var result = await helper.GetRoutesForAll(MAP_ID, S1, (RouteType)999, null);

        Assert.NotNull(result);
        var route = result!.Single();
        Assert.NotNull(route.Route);
        // Default cost is 1.0 for every system (same as Shortest)
        Assert.Equal(new[] { S1, S2, S3, S4 }, route.Route);
        Assert.Equal(4, route.RouteLength);
    }
}

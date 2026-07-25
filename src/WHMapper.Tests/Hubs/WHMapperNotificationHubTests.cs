using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Hubs;
using WHMapper.Services.EveMapper;
using WHMapper.Services.Metrics;

namespace WHMapper.Tests.Hubs;

public class WHMapperNotificationHubTests : IDisposable
{
    private readonly Dictionary<string, long> _counterValues = new();
    private readonly Dictionary<string, int> _gaugeValues = new();
    private readonly WHMapperStoreMetrics _meters;

    public WHMapperNotificationHubTests()
    {
        ResetHubStaticState();

        var loggerMock = new Mock<ILogger<WHMapperStoreMetrics>>();
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["WHMapperStoreMeterName"]).Returns($"test-hub-meter-{Guid.NewGuid()}");
        var meterFactory = new CountingMeterFactory(_counterValues, _gaugeValues);
        _meters = new WHMapperStoreMetrics(loggerMock.Object, meterFactory, configMock.Object);
    }

    public void Dispose() => ResetHubStaticState();

    private static void ResetHubStaticState()
    {
        var hubType = typeof(WHMapperNotificationHub);

        var connectionsField = hubType.GetField("_connections", BindingFlags.NonPublic | BindingFlags.Static);
        var connectionMapping = connectionsField!.GetValue(null)!;
        var innerDictField = connectionMapping.GetType()
            .GetField("_connections", BindingFlags.NonPublic | BindingFlags.Instance);
        var innerDict = innerDictField!.GetValue(connectionMapping)!;
        lock (innerDict)
        {
            innerDict.GetType().GetMethod("Clear")!.Invoke(innerDict, null);
        }

        var positionsField = hubType.GetField("_connectedUserPosition", BindingFlags.NonPublic | BindingFlags.Static);
        var concurrentDict = positionsField!.GetValue(null)!;
        concurrentDict.GetType().GetMethod("Clear")!.Invoke(concurrentDict, null);

        var mapConnectionsField = hubType.GetField("_mapConnections", BindingFlags.NonPublic | BindingFlags.Static);
        var mapConnections = mapConnectionsField!.GetValue(null)!;
        mapConnections.GetType().GetMethod("Clear")!.Invoke(mapConnections, null);

        var authorizedMapsField = hubType.GetField("_authorizedMaps", BindingFlags.NonPublic | BindingFlags.Static);
        var authorizedMaps = authorizedMapsField!.GetValue(null)!;
        authorizedMaps.GetType().GetMethod("Clear")!.Invoke(authorizedMaps, null);

        var authorizedInstancesField = hubType.GetField("_authorizedInstances", BindingFlags.NonPublic | BindingFlags.Static);
        var authorizedInstances = authorizedInstancesField!.GetValue(null)!;
        authorizedInstances.GetType().GetMethod("Clear")!.Invoke(authorizedInstances, null);
    }

    // Notification sink of the last hub built, so tests can assert what was broadcast.
    private Mock<IWHMapperNotificationHub> _lastNotificationTarget = new();

    private WHMapperNotificationHub CreateHub(
        int accountId,
        string connectionId,
        bool mapAccessAuthorized = true,
        bool instanceAccessAuthorized = true,
        bool isInstanceAdmin = true,
        IEnumerable<int>? accessibleInstanceIds = null,
        int? mapInstanceId = 7)
    {
        var accessHelperMock = new Mock<IEveMapperAccessHelper>();
        accessHelperMock
            .Setup(h => h.IsEveMapperMapAccessAuthorized(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(mapAccessAuthorized);
        accessHelperMock
            .Setup(h => h.IsEveMapperInstanceAccessAuthorized(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(instanceAccessAuthorized);
        accessHelperMock
            .Setup(h => h.IsInstanceAdminAuthorized(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(isInstanceAdmin);
        accessHelperMock
            .Setup(h => h.GetAccessibleInstanceIdsAsync(It.IsAny<int>()))
            .ReturnsAsync(accessibleInstanceIds ?? Array.Empty<int>());
        accessHelperMock
            .Setup(h => h.GetMapInstanceIdAsync(It.IsAny<int>()))
            .ReturnsAsync(mapInstanceId);

        var hub = new WHMapperNotificationHub(_meters, accessHelperMock.Object);

        var contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.UserIdentifier).Returns(accountId == 0 ? null : $"prefix:scheme:{accountId}");
        contextMock.Setup(c => c.ConnectionId).Returns(connectionId);
        hub.Context = contextMock.Object;

        var notifTarget = new Mock<IWHMapperNotificationHub>();
        _lastNotificationTarget = notifTarget;
        var clientsMock = new Mock<IHubCallerClients<IWHMapperNotificationHub>>();
        clientsMock.Setup(c => c.AllExcept(It.IsAny<IReadOnlyList<string>>())).Returns(notifTarget.Object);
        clientsMock.Setup(c => c.OthersInGroup(It.IsAny<string>())).Returns(notifTarget.Object);
        clientsMock.Setup(c => c.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(notifTarget.Object);
        hub.Clients = clientsMock.Object;

        var groupsMock = new Mock<IGroupManager>();
        groupsMock.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        groupsMock.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        hub.Groups = groupsMock.Object;

        return hub;
    }

    [Fact]
    public async Task OnConnectedAsync_SingleConnection_IncrementsConnectCounterOnce()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");

        await hub.OnConnectedAsync();

        Assert.Equal(1, _counterValues["users-connected"]);
        Assert.False(_counterValues.ContainsKey("users-disconnected"));
    }

    [Fact]
    public async Task OnConnectedAsync_TwoConnectionsSameAccount_IncrementsCounterOnlyOnce()
    {
        var hub1 = CreateHub(accountId: 123, connectionId: "conn-1");
        var hub2 = CreateHub(accountId: 123, connectionId: "conn-2");

        await hub1.OnConnectedAsync();
        await hub2.OnConnectedAsync();

        Assert.Equal(1, _counterValues["users-connected"]);
    }

    [Fact]
    public async Task OnConnectedAsync_DifferentAccounts_IncrementsCounterPerAccount()
    {
        var hubA = CreateHub(accountId: 111, connectionId: "conn-A");
        var hubB = CreateHub(accountId: 222, connectionId: "conn-B");

        await hubA.OnConnectedAsync();
        await hubB.OnConnectedAsync();

        Assert.Equal(2, _counterValues["users-connected"]);
    }

    [Fact]
    public async Task OnConnectedAsync_AccountIdZero_DoesNotIncrementCounter()
    {
        var hub = CreateHub(accountId: 0, connectionId: "conn-1");

        await hub.OnConnectedAsync();

        Assert.False(_counterValues.ContainsKey("users-connected"));
    }

    [Fact]
    public async Task OnDisconnectedAsync_LastConnection_IncrementsDisconnectCounter()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();

        await hub.OnDisconnectedAsync(null);

        Assert.Equal(1, _counterValues["users-connected"]);
        Assert.Equal(1, _counterValues["users-disconnected"]);
    }

    [Fact]
    public async Task OnDisconnectedAsync_NotLastConnection_DoesNotIncrementDisconnectCounter()
    {
        var hub1 = CreateHub(accountId: 123, connectionId: "conn-1");
        var hub2 = CreateHub(accountId: 123, connectionId: "conn-2");
        await hub1.OnConnectedAsync();
        await hub2.OnConnectedAsync();

        await hub1.OnDisconnectedAsync(null);

        Assert.Equal(1, _counterValues["users-connected"]);
        Assert.False(_counterValues.ContainsKey("users-disconnected"));
    }

    [Fact]
    public async Task OnDisconnectedAsync_AllConnectionsRemoved_IncrementsDisconnectExactlyOnce()
    {
        var hub1 = CreateHub(accountId: 123, connectionId: "conn-1");
        var hub2 = CreateHub(accountId: 123, connectionId: "conn-2");
        await hub1.OnConnectedAsync();
        await hub2.OnConnectedAsync();

        await hub1.OnDisconnectedAsync(null);
        await hub2.OnDisconnectedAsync(null);

        Assert.Equal(1, _counterValues["users-connected"]);
        Assert.Equal(1, _counterValues["users-disconnected"]);
    }

    [Fact]
    public async Task OnDisconnectedAsync_AccountIdZero_DoesNotIncrementCounter()
    {
        var hub = CreateHub(accountId: 0, connectionId: "conn-1");

        await hub.OnDisconnectedAsync(null);

        Assert.False(_counterValues.ContainsKey("users-disconnected"));
    }

    [Fact]
    public async Task ConcurrentConnectionsSameAccount_IncrementsConnectExactlyOnce()
    {
        const int parallelTasks = 50;
        var hubs = Enumerable.Range(0, parallelTasks)
            .Select(i => CreateHub(accountId: 123, connectionId: $"conn-{i}"))
            .ToArray();

        await Task.WhenAll(hubs.Select(h => h.OnConnectedAsync()));

        Assert.Equal(1, _counterValues["users-connected"]);
    }

    [Fact]
    public async Task ConcurrentConnectionsAndDisconnections_ExactlyOneConnectAndOneDisconnect()
    {
        const int parallelTasks = 50;
        var hubs = Enumerable.Range(0, parallelTasks)
            .Select(i => CreateHub(accountId: 123, connectionId: $"conn-{i}"))
            .ToArray();

        await Task.WhenAll(hubs.Select(h => h.OnConnectedAsync()));
        await Task.WhenAll(hubs.Select(h => h.OnDisconnectedAsync(null)));

        Assert.Equal(1, _counterValues["users-connected"]);
        Assert.Equal(1, _counterValues["users-disconnected"]);
    }

    [Fact]
    public async Task GetTotalConnectedUsers_NoConnections_ReturnsZero()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");

        int total = await hub.GetTotalConnectedUsers();

        Assert.Equal(0, total);
    }

    [Fact]
    public async Task GetTotalConnectedUsers_AfterConnections_ReturnsDistinctAccountCount()
    {
        var hubA1 = CreateHub(accountId: 111, connectionId: "conn-A1");
        var hubA2 = CreateHub(accountId: 111, connectionId: "conn-A2");
        var hubB = CreateHub(accountId: 222, connectionId: "conn-B");

        await hubA1.OnConnectedAsync();
        await hubA2.OnConnectedAsync();
        await hubB.OnConnectedAsync();

        Assert.Equal(2, await hubA1.GetTotalConnectedUsers());
    }

    [Fact]
    public async Task GetUserCountOnMap_UnknownMap_ReturnsZero()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");

        int count = await hub.GetUserCountOnMap(42);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task SendUserOnMapConnected_AddsUserToMap()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();
        await hub.JoinMap(42);

        await hub.SendUserOnMapConnected(42);

        Assert.Equal(1, await hub.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task SendUserOnMapConnected_SameAccountTwoConnections_CountedOnce()
    {
        var hub1 = CreateHub(accountId: 123, connectionId: "conn-1");
        var hub2 = CreateHub(accountId: 123, connectionId: "conn-2");
        await hub1.OnConnectedAsync();
        await hub2.OnConnectedAsync();
        await hub1.JoinMap(42);
        await hub2.JoinMap(42);

        await hub1.SendUserOnMapConnected(42);
        await hub2.SendUserOnMapConnected(42);

        Assert.Equal(1, await hub1.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task SendUserOnMapConnected_DifferentAccounts_CountedSeparately()
    {
        var hubA = CreateHub(accountId: 111, connectionId: "conn-A");
        var hubB = CreateHub(accountId: 222, connectionId: "conn-B");
        await hubA.OnConnectedAsync();
        await hubB.OnConnectedAsync();
        await hubA.JoinMap(42);
        await hubB.JoinMap(42);

        await hubA.SendUserOnMapConnected(42);
        await hubB.SendUserOnMapConnected(42);

        Assert.Equal(2, await hubA.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task SendUserOnMapDisconnected_RemovesUserFromMap()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();
        await hub.JoinMap(42);
        await hub.SendUserOnMapConnected(42);

        await hub.SendUserOnMapDisconnected(42);

        Assert.Equal(0, await hub.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task OnDisconnectedAsync_CleansUpMapPresence_WithoutExplicitLeave()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();
        await hub.JoinMap(42);
        await hub.SendUserOnMapConnected(42);

        await hub.OnDisconnectedAsync(null);

        Assert.Equal(0, await hub.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task OnDisconnectedAsync_OneOfTwoConnections_KeepsAccountOnMap()
    {
        var hub1 = CreateHub(accountId: 123, connectionId: "conn-1");
        var hub2 = CreateHub(accountId: 123, connectionId: "conn-2");
        await hub1.OnConnectedAsync();
        await hub2.OnConnectedAsync();
        await hub1.JoinMap(42);
        await hub2.JoinMap(42);
        await hub1.SendUserOnMapConnected(42);
        await hub2.SendUserOnMapConnected(42);

        await hub1.OnDisconnectedAsync(null);

        // Queried through the surviving connection: a disconnected connection loses its map
        // authorization and can no longer read map-scoped counts.
        Assert.Equal(1, await hub2.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task SendUserOnMapConnected_DifferentMaps_AreIndependent()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();
        await hub.JoinMap(42);
        await hub.JoinMap(99);

        await hub.SendUserOnMapConnected(42);
        await hub.SendUserOnMapConnected(99);

        Assert.Equal(1, await hub.GetUserCountOnMap(42));
        Assert.Equal(1, await hub.GetUserCountOnMap(99));
    }

    [Fact]
    public async Task JoinMap_AccessNotAuthorized_ThrowsAndDoesNotJoin()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1", mapAccessAuthorized: false);
        await hub.OnConnectedAsync();

        await Assert.ThrowsAsync<HubException>(() => hub.JoinMap(42));

        // Presence updates from an unauthorized connection must be ignored.
        await hub.SendUserOnMapConnected(42);
        Assert.Equal(0, await hub.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task JoinInstance_AccessNotAuthorized_ThrowsAndDoesNotJoin()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1", instanceAccessAuthorized: false);
        await hub.OnConnectedAsync();

        await Assert.ThrowsAsync<HubException>(() => hub.JoinInstance(7));

        // An unauthorized connection must not be able to broadcast into that instance either.
        await hub.SendMapAdded(7, 42);
        _lastNotificationTarget.Verify(t => t.NotifyMapAdded(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SendMapAdded_WithoutJoiningInstance_IsIgnored()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();

        // No JoinInstance/JoinMap call -> the connection was never authorized for instance 7.
        await hub.SendMapAdded(7, 42);

        _lastNotificationTarget.Verify(t => t.NotifyMapAdded(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SendMapAccessRemoved_NotInstanceAdmin_IsIgnored()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1", isInstanceAdmin: false);
        await hub.OnConnectedAsync();
        await hub.JoinInstance(7);

        await hub.SendMapAccessRemoved(7, 42, 5);

        _lastNotificationTarget.Verify(
            t => t.NotifyMapAccessRemoved(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SendMapAccessRemoved_InstanceAdmin_BroadcastsToInstanceGroup()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();
        await hub.JoinInstance(7);

        await hub.SendMapAccessRemoved(7, 42, 5);

        _lastNotificationTarget.Verify(t => t.NotifyMapAccessRemoved(123, 42, 5), Times.Once);
    }

    [Fact]
    public async Task SendInstanceRemoved_NotAdminAtJoinTime_IsIgnored()
    {
        // Admin status is captured when joining; a plain member must not be able to forge the event.
        var hub = CreateHub(accountId: 123, connectionId: "conn-1", isInstanceAdmin: false);
        await hub.OnConnectedAsync();
        await hub.JoinInstance(7);

        await hub.SendInstanceRemoved(7);

        _lastNotificationTarget.Verify(t => t.NotifyInstanceRemoved(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task JoinMap_AlsoJoinsOwningInstance()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1", mapInstanceId: 7);
        await hub.OnConnectedAsync();
        await hub.JoinMap(42);

        // Instance membership was derived server-side from the map, so the broadcast goes through.
        await hub.SendMapAccessRemoved(7, 42, 5);

        _lastNotificationTarget.Verify(t => t.NotifyMapAccessRemoved(123, 42, 5), Times.Once);
    }

    [Fact]
    public async Task GetConnectedUsersPosition_ExcludesMapsTheConnectionCannotSee()
    {
        var owner = CreateHub(accountId: 123, connectionId: "conn-1");
        await owner.OnConnectedAsync();
        await owner.JoinMap(42);
        await owner.SendUserPosition(42, 1000);

        var stranger = CreateHub(accountId: 456, connectionId: "conn-2");
        await stranger.OnConnectedAsync();
        await stranger.JoinMap(99);

        var visible = await stranger.GetConnectedUsersPosition();

        Assert.DoesNotContain(123, visible.Keys);
    }

    [Fact]
    public async Task GetConnectedUsersPosition_IncludesUsersOnASharedMap()
    {
        var owner = CreateHub(accountId: 123, connectionId: "conn-1");
        await owner.OnConnectedAsync();
        await owner.JoinMap(42);
        await owner.SendUserPosition(42, 1000);

        var peer = CreateHub(accountId: 456, connectionId: "conn-2");
        await peer.OnConnectedAsync();
        await peer.JoinMap(42);

        var visible = await peer.GetConnectedUsersPosition();

        Assert.Equal(new KeyValuePair<int, int>(42, 1000), visible[123]);
    }

    [Fact]
    public async Task GetUserCountOnMap_WithoutJoiningMap_ReturnsZero()
    {
        var member = CreateHub(accountId: 123, connectionId: "conn-1");
        await member.OnConnectedAsync();
        await member.JoinMap(42);
        await member.SendUserOnMapConnected(42);

        var stranger = CreateHub(accountId: 456, connectionId: "conn-2");
        await stranger.OnConnectedAsync();

        Assert.Equal(0, await stranger.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task SendUserOnMapConnected_WithoutJoiningMap_IsIgnored()
    {
        var hub = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub.OnConnectedAsync();

        // No JoinMap call -> connection was never authorized for this map.
        await hub.SendUserOnMapConnected(42);

        Assert.Equal(0, await hub.GetUserCountOnMap(42));
    }

    [Fact]
    public async Task Reconnect_AfterFullDisconnect_IncrementsConnectAgain()
    {
        var hub1 = CreateHub(accountId: 123, connectionId: "conn-1");
        await hub1.OnConnectedAsync();
        await hub1.OnDisconnectedAsync(null);

        var hub2 = CreateHub(accountId: 123, connectionId: "conn-2");
        await hub2.OnConnectedAsync();

        Assert.Equal(2, _counterValues["users-connected"]);
        Assert.Equal(1, _counterValues["users-disconnected"]);
    }

    private sealed class CountingMeterFactory : IMeterFactory
    {
        private readonly Dictionary<string, long> _counterValues;
        private readonly Dictionary<string, int> _gaugeValues;
        private readonly object _lock = new();

        public CountingMeterFactory(Dictionary<string, long> counterValues, Dictionary<string, int> gaugeValues)
        {
            _counterValues = counterValues;
            _gaugeValues = gaugeValues;
        }

        public Meter Create(MeterOptions options)
        {
            var meter = new Meter(options.Name, options.Version, Array.Empty<KeyValuePair<string, object?>>(), this);

            var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter == meter)
                    listener.EnableMeasurementEvents(instrument, null);
            };
            listener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
            {
                lock (_lock)
                {
                    if (instrument is Counter<int>)
                    {
                        if (!_counterValues.ContainsKey(instrument.Name))
                            _counterValues[instrument.Name] = 0;
                        _counterValues[instrument.Name] += measurement;
                    }
                    else
                    {
                        _gaugeValues[instrument.Name] = measurement;
                    }
                }
            });
            listener.Start();

            return meter;
        }

        public void Dispose() { }
    }
}

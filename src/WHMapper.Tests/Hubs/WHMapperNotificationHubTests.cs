using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Hubs;
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
    }

    private WHMapperNotificationHub CreateHub(int accountId, string connectionId)
    {
        var hub = new WHMapperNotificationHub(_meters);

        var contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.UserIdentifier).Returns(accountId == 0 ? null : $"prefix:scheme:{accountId}");
        contextMock.Setup(c => c.ConnectionId).Returns(connectionId);
        hub.Context = contextMock.Object;

        var notifTarget = new Mock<IWHMapperNotificationHub>();
        var clientsMock = new Mock<IHubCallerClients<IWHMapperNotificationHub>>();
        clientsMock.Setup(c => c.AllExcept(It.IsAny<IReadOnlyList<string>>())).Returns(notifTarget.Object);
        hub.Clients = clientsMock.Object;

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

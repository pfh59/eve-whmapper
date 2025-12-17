using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.Metrics;

namespace WHMapper.Tests.Services.Metrics;

public class WHMapperStoreMetricsTest
{
    private readonly Mock<ILogger<WHMapperStoreMetrics>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Dictionary<string, long> _counterValues;
    private readonly Dictionary<string, int> _gaugeValues;

    public WHMapperStoreMetricsTest()
    {
        _loggerMock = new Mock<ILogger<WHMapperStoreMetrics>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["WHMapperStoreMeterName"]).Returns("test-meter");
        _counterValues = new Dictionary<string, long>();
        _gaugeValues = new Dictionary<string, int>();
    }

    private class TestMeterFactory : IMeterFactory
    {
        private readonly Dictionary<string, long> _counterValues;
        private readonly Dictionary<string, int> _gaugeValues;

        public TestMeterFactory(Dictionary<string, long> counterValues, Dictionary<string, int> gaugeValues)
        {
            _counterValues = counterValues;
            _gaugeValues = gaugeValues;
        }

        public Meter Create(MeterOptions options)
        {
            var meter = new Meter(options.Name, options.Version, Array.Empty<KeyValuePair<string, object?>>(), this);
            
            // Hook into the meter's instrument publishing to capture counter values
            var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter == meter)
                {
                    if (instrument is Counter<int>)
                    {
                        listener.EnableMeasurementEvents(instrument, null);
                    }
                    else if (instrument.GetType().Name.Contains("ObservableGauge"))
                    {
                        listener.EnableMeasurementEvents(instrument, null);
                    }
                }
            };

            listener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
            {
                var name = instrument.Name;
                if (instrument is Counter<int>)
                {
                    if (!_counterValues.ContainsKey(name))
                        _counterValues[name] = 0;
                    _counterValues[name] += measurement;
                }
                else
                {
                    _gaugeValues[name] = measurement;
                }
            });

            listener.Start();
            
            return meter;
        }

        public void Dispose() { }
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldCreateInstance()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        Assert.NotNull(metrics);
    }

    [Fact]
    public void Constructor_WithNullMeterName_ShouldThrowArgumentNullException()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        _configurationMock.Setup(c => c["WHMapperStoreMeterName"]).Returns((string?)null);

        Assert.Throws<ArgumentNullException>(() => 
            new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object));
    }

    [Fact]
    public async Task InitializeTotalsAsync_WithValidRepositories_ShouldInitializeTotals()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        var mapRepoMock = new Mock<IWHMapRepository>();
        var systemRepoMock = new Mock<IWHSystemRepository>();
        var linkRepoMock = new Mock<IWHSystemLinkRepository>();
        var signatureRepoMock = new Mock<IWHSignatureRepository>();
        var noteRepoMock = new Mock<IWHNoteRepository>();
        var jumpLogRepoMock = new Mock<IWHJumpLogRepository>();

        mapRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(10);
        systemRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(20);
        linkRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(30);
        signatureRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(40);
        noteRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(50);
        jumpLogRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(60);

        await metrics.InitializeTotalsAsync(mapRepoMock.Object, systemRepoMock.Object, linkRepoMock.Object,
            signatureRepoMock.Object, noteRepoMock.Object, jumpLogRepoMock.Object);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Initializing WHMapperStore metrics totals")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("WHMapperStore metrics totals initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeTotalsAsync_WithRepositoryError_ShouldLogError()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        var mapRepoMock = new Mock<IWHMapRepository>();
        var systemRepoMock = new Mock<IWHSystemRepository>();
        var linkRepoMock = new Mock<IWHSystemLinkRepository>();
        var signatureRepoMock = new Mock<IWHSignatureRepository>();
        var noteRepoMock = new Mock<IWHNoteRepository>();
        var jumpLogRepoMock = new Mock<IWHJumpLogRepository>();

        mapRepoMock.Setup(r => r.GetCountAsync()).ThrowsAsync(new Exception("Test error"));

        await metrics.InitializeTotalsAsync(mapRepoMock.Object, systemRepoMock.Object, linkRepoMock.Object,
            signatureRepoMock.Object, noteRepoMock.Object, jumpLogRepoMock.Object);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ConnectUser_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.ConnectUser();

        Assert.True(_counterValues.ContainsKey("users-connected"));
        Assert.Equal(1, _counterValues["users-connected"]);
    }

    [Fact]
    public void DisconnectUser_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.ConnectUser();
        metrics.DisconnectUser();

        Assert.True(_counterValues.ContainsKey("users-disconnected"));
        Assert.Equal(1, _counterValues["users-disconnected"]);
    }

    [Fact]
    public void CreateMap_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateMap();

        Assert.True(_counterValues.ContainsKey("maps-created"));
        Assert.Equal(1, _counterValues["maps-created"]);
    }

    [Fact]
    public void DeleteMap_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteMap();

        Assert.True(_counterValues.ContainsKey("maps-deleted"));
        Assert.Equal(1, _counterValues["maps-deleted"]);
    }

    [Fact]
    public void DeleteMaps_ShouldIncrementCountersBySpecifiedAmount()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteMaps(5);

        Assert.True(_counterValues.ContainsKey("maps-deleted"));
        Assert.Equal(5, _counterValues["maps-deleted"]);
    }

    [Fact]
    public void DeleteMaps_WithZeroCount_ShouldNotChangeCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteMaps(0);

        Assert.False(_counterValues.ContainsKey("maps-deleted") && _counterValues["maps-deleted"] > 0);
    }

    [Fact]
    public void AddSystem_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.AddSystem();

        Assert.True(_counterValues.ContainsKey("systems-added"));
        Assert.Equal(1, _counterValues["systems-added"]);
    }

    [Fact]
    public void DeleteSystem_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSystem();

        Assert.True(_counterValues.ContainsKey("systems-deleted"));
        Assert.Equal(1, _counterValues["systems-deleted"]);
    }

    [Fact]
    public void DeleteSystems_ShouldIncrementCountersBySpecifiedAmount()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSystems(3);

        Assert.True(_counterValues.ContainsKey("systems-deleted"));
        Assert.Equal(3, _counterValues["systems-deleted"]);
    }

    [Fact]
    public void AddLink_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.AddLink();

        Assert.True(_counterValues.ContainsKey("links-added"));
        Assert.Equal(1, _counterValues["links-added"]);
    }

    [Fact]
    public void DeleteLink_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteLink();

        Assert.True(_counterValues.ContainsKey("links-deleted"));
        Assert.Equal(1, _counterValues["links-deleted"]);
    }

    [Fact]
    public void DeleteLinks_ShouldIncrementCountersBySpecifiedAmount()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteLinks(7);

        Assert.True(_counterValues.ContainsKey("links-deleted"));
        Assert.Equal(7, _counterValues["links-deleted"]);
    }

    [Fact]
    public void CreateSignature_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateSignature();

        Assert.True(_counterValues.ContainsKey("signatures-created"));
        Assert.Equal(1, _counterValues["signatures-created"]);
    }

    [Fact]
    public void DeleteSignature_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSignature();

        Assert.True(_counterValues.ContainsKey("signatures-deleted"));
        Assert.Equal(1, _counterValues["signatures-deleted"]);
    }

    [Fact]
    public void DeleteSignatures_ShouldIncrementCountersBySpecifiedAmount()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSignatures(4);

        Assert.True(_counterValues.ContainsKey("signatures-deleted"));
        Assert.Equal(4, _counterValues["signatures-deleted"]);
    }

    [Fact]
    public void CreateNote_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateNote();

        Assert.True(_counterValues.ContainsKey("notes-created"));
        Assert.Equal(1, _counterValues["notes-created"]);
    }

    [Fact]
    public void DeleteNote_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteNote();

        Assert.True(_counterValues.ContainsKey("notes-deleted"));
        Assert.Equal(1, _counterValues["notes-deleted"]);
    }

    [Fact]
    public void DeleteNotes_ShouldIncrementCountersBySpecifiedAmount()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteNotes(6);

        Assert.True(_counterValues.ContainsKey("notes-deleted"));
        Assert.Equal(6, _counterValues["notes-deleted"]);
    }

    [Fact]
    public void CreateJumpLog_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateJumpLog();

        Assert.True(_counterValues.ContainsKey("jumplogs-created"));
        Assert.Equal(1, _counterValues["jumplogs-created"]);
    }

    [Fact]
    public void DeleteJumpLog_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteJumpLog();

        Assert.True(_counterValues.ContainsKey("jumplogs-deleted"));
        Assert.Equal(1, _counterValues["jumplogs-deleted"]);
    }

    [Fact]
    public void DeleteJumpLogs_ShouldIncrementCountersBySpecifiedAmount()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteJumpLogs(8);

        Assert.True(_counterValues.ContainsKey("jumplogs-deleted"));
        Assert.Equal(8, _counterValues["jumplogs-deleted"]);
    }

    [Fact]
    public void MultipleOperations_ShouldAccumulateCorrectly()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateMap();
        metrics.CreateMap();
        metrics.CreateMap();
        metrics.DeleteMap();

        Assert.Equal(3, _counterValues["maps-created"]);
        Assert.Equal(1, _counterValues["maps-deleted"]);
    }

    [Fact]
    public void AllMetricsOperations_ShouldWorkIndependently()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.ConnectUser();
        metrics.CreateMap();
        metrics.AddSystem();
        metrics.AddLink();
        metrics.CreateSignature();
        metrics.CreateNote();
        metrics.CreateJumpLog();

        Assert.Equal(1, _counterValues["users-connected"]);
        Assert.Equal(1, _counterValues["maps-created"]);
        Assert.Equal(1, _counterValues["systems-added"]);
        Assert.Equal(1, _counterValues["links-added"]);
        Assert.Equal(1, _counterValues["signatures-created"]);
        Assert.Equal(1, _counterValues["notes-created"]);
        Assert.Equal(1, _counterValues["jumplogs-created"]);
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldHandleCorrectly()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        var tasks = new List<Task>();

        
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => metrics.CreateMap()));
            tasks.Add(Task.Run(() => metrics.AddSystem()));
            tasks.Add(Task.Run(() => metrics.AddLink()));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(10, _counterValues["maps-created"]);
        Assert.Equal(10, _counterValues["systems-added"]);
        Assert.Equal(10, _counterValues["links-added"]);
    }

    [Fact]
    public void DeleteMaps_WithLargeCount_ShouldHandleCorrectly()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteMaps(1000);

        Assert.Equal(1000, _counterValues["maps-deleted"]);
    }

    [Fact]
    public void BalancedCreateDeleteOperations_ShouldMaintainCorrectCounters()
    {
        var meterFactory = new TestMeterFactory(_counterValues, _gaugeValues);
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateMap();
        metrics.CreateMap();
        metrics.CreateMap();
        metrics.DeleteMaps(3);

        Assert.Equal(3, _counterValues["maps-created"]);
        Assert.Equal(3, _counterValues["maps-deleted"]);
    }
}
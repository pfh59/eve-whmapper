using System;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
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
using Xunit;

namespace WHMapper.Tests.Services.Metrics;

public class WHMapperStoreMetricsTest : IDisposable
{
    private readonly Mock<ILogger<WHMapperStoreMetrics>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly MeterListener _meterListener;
    private readonly Dictionary<string, long> _counterValues;
    private readonly Dictionary<string, long> _upDownCounterValues;
    private readonly string _meterName = "WHMapperStoreTestMeter";

    public WHMapperStoreMetricsTest()
    {
        _loggerMock = new Mock<ILogger<WHMapperStoreMetrics>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["WHMapperStoreMeterName"]).Returns(_meterName);

        _counterValues = new Dictionary<string, long>();
        _upDownCounterValues = new Dictionary<string, long>();

        _meterListener = new MeterListener();
        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == _meterName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        _meterListener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name.StartsWith("total-"))
            {
                if (!_upDownCounterValues.ContainsKey(instrument.Name))
                    _upDownCounterValues[instrument.Name] = 0;
                _upDownCounterValues[instrument.Name] += measurement;
            }
            else
            {
                if (!_counterValues.ContainsKey(instrument.Name))
                    _counterValues[instrument.Name] = 0;
                _counterValues[instrument.Name] += measurement;
            }
        });

        _meterListener.Start();
    }

    public void Dispose()
    {
        _meterListener?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_ShouldInitializeAllMetrics()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        Assert.NotNull(metrics);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenMeterNameIsNull()
    {
        var meterFactory = new TestMeterFactory();
        _configurationMock.Setup(c => c["WHMapperStoreMeterName"]).Returns((string?)null);

        Assert.Throws<ArgumentNullException>(() =>
            new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object));
    }

    [Fact]
    public async Task InitializeTotalsAsync_ShouldSetCorrectValues()
    {
        var meterFactory = new TestMeterFactory();
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

        Assert.Equal(10, _upDownCounterValues["total-maps"]);
        Assert.Equal(20, _upDownCounterValues["total-systems"]);
        Assert.Equal(30, _upDownCounterValues["total-links"]);
        Assert.Equal(40, _upDownCounterValues["total-signatures"]);
        Assert.Equal(50, _upDownCounterValues["total-notes"]);
        Assert.Equal(60, _upDownCounterValues["total-jumplogs"]);
    }

    [Fact]
    public async Task InitializeTotalsAsync_ShouldHandleException()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        var mapRepoMock = new Mock<IWHMapRepository>();
        var systemRepoMock = new Mock<IWHSystemRepository>();
        var linkRepoMock = new Mock<IWHSystemLinkRepository>();
        var signatureRepoMock = new Mock<IWHSignatureRepository>();
        var noteRepoMock = new Mock<IWHNoteRepository>();
        var jumpLogRepoMock = new Mock<IWHJumpLogRepository>();

        mapRepoMock.Setup(r => r.GetCountAsync()).ThrowsAsync(new Exception("Test exception"));

        await metrics.InitializeTotalsAsync(mapRepoMock.Object, systemRepoMock.Object, linkRepoMock.Object, signatureRepoMock.Object, noteRepoMock.Object, jumpLogRepoMock.Object);

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
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.ConnectUser();

        Assert.Equal(1, _counterValues["users-connected"]);
        Assert.Equal(1, _upDownCounterValues["total-users"]);
    }

    [Fact]
    public void DisconnectUser_ShouldIncrementAndDecrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DisconnectUser();

        Assert.Equal(1, _counterValues["users-disconnected"]);
        Assert.Equal(-1, _upDownCounterValues["total-users"]);
    }

    [Fact]
    public void CreateMap_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateMap();

        Assert.Equal(1, _counterValues["maps-created"]);
        Assert.Equal(1, _upDownCounterValues["total-maps"]);
    }

    [Fact]
    public void DeleteMap_ShouldIncrementAndDecrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteMap();

        Assert.Equal(1, _counterValues["maps-deleted"]);
        Assert.Equal(-1, _upDownCounterValues["total-maps"]);
    }

    [Fact]
    public void DeleteMaps_ShouldIncrementAndDecrementCountersByCount()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteMaps(5);

        Assert.Equal(5, _counterValues["maps-deleted"]);
        Assert.Equal(-5, _upDownCounterValues["total-maps"]);
    }

    [Fact]
    public void AddSystem_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.AddSystem();

        Assert.Equal(1, _counterValues["systems-added"]);
        Assert.Equal(1, _upDownCounterValues["total-systems"]);
    }

    [Fact]
    public void DeleteSystem_ShouldIncrementAndDecrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSystem();

        Assert.Equal(1, _counterValues["systems-deleted"]);
        Assert.Equal(-1, _upDownCounterValues["total-systems"]);
    }

    [Fact]
    public void DeleteSystems_ShouldIncrementAndDecrementCountersByCount()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSystems(3);

        Assert.Equal(3, _counterValues["systems-deleted"]);
        Assert.Equal(-3, _upDownCounterValues["total-systems"]);
    }

    [Fact]
    public void AddLink_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.AddLink();

        Assert.Equal(1, _counterValues["links-added"]);
        Assert.Equal(1, _upDownCounterValues["total-links"]);
    }

    [Fact]
    public void DeleteLink_ShouldIncrementAndDecrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteLink();

        Assert.Equal(1, _counterValues["links-deleted"]);
        Assert.Equal(-1, _upDownCounterValues["total-links"]);
    }

    [Fact]
    public void DeleteLinks_ShouldIncrementAndDecrementCountersByCount()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteLinks(7);

        Assert.Equal(7, _counterValues["links-deleted"]);
        Assert.Equal(-7, _upDownCounterValues["total-links"]);
    }

    [Fact]
    public void CreateSignature_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateSignature();

        Assert.Equal(1, _counterValues["signatures-created"]);
        Assert.Equal(1, _upDownCounterValues["total-signatures"]);
    }

    [Fact]
    public void DeleteSignature_ShouldIncrementAndDecrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSignature();

        Assert.Equal(1, _counterValues["signatures-deleted"]);
        Assert.Equal(-1, _upDownCounterValues["total-signatures"]);
    }

    [Fact]
    public void DeleteSignatures_ShouldIncrementAndDecrementCountersByCount()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteSignatures(4);

        Assert.Equal(4, _counterValues["signatures-deleted"]);
        Assert.Equal(-4, _upDownCounterValues["total-signatures"]);
    }

    [Fact]
    public void CreateNote_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateNote();

        Assert.Equal(1, _counterValues["notes-created"]);
        Assert.Equal(1, _upDownCounterValues["total-notes"]);
    }

    [Fact]
    public void DeleteNote_ShouldIncrementAndDecrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteNote();

        Assert.Equal(1, _counterValues["notes-deleted"]);
        Assert.Equal(-1, _upDownCounterValues["total-notes"]);
    }

    [Fact]
    public void DeleteNotes_ShouldIncrementAndDecrementCountersByCount()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteNotes(6);

        Assert.Equal(6, _counterValues["notes-deleted"]);
        Assert.Equal(-6, _upDownCounterValues["total-notes"]);
    }

    [Fact]
    public void CreateJumpLog_ShouldIncrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.CreateJumpLog();

        Assert.Equal(1, _counterValues["jumplogs-created"]);
        Assert.Equal(1, _upDownCounterValues["total-jumplogs"]);
    }

    [Fact]
    public void DeleteJumpLog_ShouldIncrementAndDecrementCounters()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteJumpLog();

        Assert.Equal(1, _counterValues["jumplogs-deleted"]);
        Assert.Equal(-1, _upDownCounterValues["total-jumplogs"]);
    }

    [Fact]
    public void DeleteJumpLogs_ShouldIncrementAndDecrementCountersByCount()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.DeleteJumpLogs(8);

        Assert.Equal(8, _counterValues["jumplogs-deleted"]);
        Assert.Equal(-8, _upDownCounterValues["total-jumplogs"]);
    }

    [Fact]
    public void MultipleOperations_ShouldAccumulateCorrectly()
    {
        var meterFactory = new TestMeterFactory();
        var metrics = new WHMapperStoreMetrics(_loggerMock.Object, meterFactory, _configurationMock.Object);

        metrics.ConnectUser();
        metrics.ConnectUser();
        metrics.DisconnectUser();
        metrics.CreateMap();
        metrics.CreateMap();
        metrics.DeleteMap();

        Assert.Equal(2, _counterValues["users-connected"]);
        Assert.Equal(1, _counterValues["users-disconnected"]);
        Assert.Equal(1, _upDownCounterValues["total-users"]);
        Assert.Equal(2, _counterValues["maps-created"]);
        Assert.Equal(1, _counterValues["maps-deleted"]);
        Assert.Equal(1, _upDownCounterValues["total-maps"]);
    }

    private class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options)
        {
            return new Meter(options);
        }

        public void Dispose()
        {
        }
    }
}
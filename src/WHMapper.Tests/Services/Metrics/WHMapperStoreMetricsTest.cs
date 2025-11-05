using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Metrics;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.Metrics;
using Xunit;

namespace WHMapper.Tests.Services.Metrics;

public class WHMapperStoreMetricsTest
{
    private readonly Mock<ILogger<WHMapperStoreMetrics>> _mockLogger;
    private readonly Mock<IMeterFactory> _mockMeterFactory;
    private readonly Mock<Meter> _mockMeter;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<Counter<int>> _mockCounter;
    private readonly Mock<UpDownCounter<int>> _mockUpDownCounter;
    private readonly Mock<IWHMapRepository> _mockMapRepository;
    private readonly Mock<IWHSystemRepository> _mockSystemRepository;
    private readonly Mock<IWHSystemLinkRepository> _mockLinkRepository;
    private readonly Mock<IWHSignatureRepository> _mockSignatureRepository;
    private readonly Mock<IWHNoteRepository> _mockNoteRepository;
    private readonly Mock<IWHJumpLogRepository> _mockJumpLogRepository;

    public WHMapperStoreMetricsTest()
    {
        _mockLogger = new Mock<ILogger<WHMapperStoreMetrics>>();
        _mockMeterFactory = new Mock<IMeterFactory>();
        _mockMeter = new Mock<Meter>("test-meter", "1.0.0");
        _mockConfiguration = new Mock<IConfiguration>();
        _mockCounter = new Mock<Counter<int>>();
        _mockUpDownCounter = new Mock<UpDownCounter<int>>();
        _mockMapRepository = new Mock<IWHMapRepository>();
        _mockSystemRepository = new Mock<IWHSystemRepository>();
        _mockLinkRepository = new Mock<IWHSystemLinkRepository>();
        _mockSignatureRepository = new Mock<IWHSignatureRepository>();
        _mockNoteRepository = new Mock<IWHNoteRepository>();
        _mockJumpLogRepository = new Mock<IWHJumpLogRepository>();
    }

    [Fact]
    public void Constructor_WithValidConfiguration_CreatesMetrics()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["WHMapperStoreMeterName"]).Returns("test-meter");
        _mockMeterFactory.Setup(f => f.Create("test-meter")).Returns(_mockMeter.Object);
        _mockMeter.Setup(m => m.CreateCounter<int>(It.IsAny<string>(), null, null))
               .Returns(_mockCounter.Object);
        _mockMeter.Setup(m => m.CreateUpDownCounter<int>(It.IsAny<string>(), null, null))
               .Returns(_mockUpDownCounter.Object);

        // Act
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Assert
        Assert.NotNull(metrics);
        _mockMeterFactory.Verify(f => f.Create("test-meter"), Times.Once);
        _mockMeter.Verify(m => m.CreateCounter<int>(It.IsAny<string>(), null, null), Times.Exactly(12));
        _mockMeter.Verify(m => m.CreateUpDownCounter<int>(It.IsAny<string>(), null, null), Times.Exactly(6));
    }

    [Fact]
    public void Constructor_WithNullMeterName_ThrowsNullReferenceException()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["WHMapperStoreMeterName"]).Returns((string)null);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => 
            new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object));
    }

    [Fact]
    public async Task InitializeTotalsAsync_WithValidRepositories_InitializesTotals()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        _mockMapRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(5);
        _mockSystemRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(10);
        _mockLinkRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(8);
        _mockSignatureRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(15);
        _mockNoteRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(3);
        _mockJumpLogRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(20);

        // Act
        await metrics.InitializeTotalsAsync(_mockMapRepository.Object, _mockSystemRepository.Object, 
            _mockLinkRepository.Object, _mockSignatureRepository.Object, _mockNoteRepository.Object, 
            _mockJumpLogRepository.Object);

        // Assert
        _mockUpDownCounter.Verify(c => c.Add(5), Times.Once); // Maps
        _mockUpDownCounter.Verify(c => c.Add(10), Times.Once); // Systems
        _mockUpDownCounter.Verify(c => c.Add(8), Times.Once); // Links
        _mockUpDownCounter.Verify(c => c.Add(15), Times.Once); // Signatures
        _mockUpDownCounter.Verify(c => c.Add(3), Times.Once); // Notes
        _mockUpDownCounter.Verify(c => c.Add(20), Times.Once); // Jump Logs
    }

    [Fact]
    public async Task InitializeTotalsAsync_WithException_LogsError()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        _mockMapRepository.Setup(r => r.GetCountAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        await metrics.InitializeTotalsAsync(_mockMapRepository.Object, _mockSystemRepository.Object, 
            _mockLinkRepository.Object, _mockSignatureRepository.Object, _mockNoteRepository.Object, 
            _mockJumpLogRepository.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing WHMapperStore metrics totals")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void ConnectUser_IncrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.ConnectUser();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(1), Times.Once);
    }

    [Fact]
    public void DisconnectUser_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DisconnectUser();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-1), Times.Once);
    }

    [Fact]
    public void CreateMap_IncrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.CreateMap();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(1), Times.Once);
    }

    [Fact]
    public void DeleteMap_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteMap();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-1), Times.Once);
    }

    [Fact]
    public void DeleteMaps_WithCount_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteMaps(5);

        // Assert
        _mockCounter.Verify(c => c.Add(5), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-5), Times.Once);
    }

    [Fact]
    public void AddSystem_IncrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.AddSystem();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(1), Times.Once);
    }

    [Fact]
    public void DeleteSystem_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteSystem();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-1), Times.Once);
    }

    [Fact]
    public void DeleteSystems_WithCount_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteSystems(3);

        // Assert
        _mockCounter.Verify(c => c.Add(3), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-3), Times.Once);
    }

    [Fact]
    public void AddLink_IncrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.AddLink();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(1), Times.Once);
    }

    [Fact]
    public void DeleteLink_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteLink();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-1), Times.Once);
    }

    [Fact]
    public void DeleteLinks_WithCount_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteLinks(7);

        // Assert
        _mockCounter.Verify(c => c.Add(7), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-7), Times.Once);
    }

    [Fact]
    public void CreateSignature_IncrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.CreateSignature();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(1), Times.Once);
    }

    [Fact]
    public void DeleteSignature_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteSignature();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-1), Times.Once);
    }

    [Fact]
    public void DeleteSignatures_WithCount_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteSignatures(4);

        // Assert
        _mockCounter.Verify(c => c.Add(4), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-4), Times.Once);
    }

    [Fact]
    public void CreateNote_IncrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.CreateNote();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(1), Times.Once);
    }

    [Fact]
    public void DeleteNote_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteNote();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-1), Times.Once);
    }

    [Fact]
    public void DeleteNotes_WithCount_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteNotes(2);

        // Assert
        _mockCounter.Verify(c => c.Add(2), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-2), Times.Once);
    }

    [Fact]
    public void CreateJumpLog_IncrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.CreateJumpLog();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(1), Times.Once);
    }

    [Fact]
    public void DeleteJumpLog_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteJumpLog();

        // Assert
        _mockCounter.Verify(c => c.Add(1), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-1), Times.Once);
    }

    [Fact]
    public void DeleteJumpLogs_WithCount_IncrementsAndDecrementsCounters()
    {
        // Arrange
        SetupValidMetrics();
        var metrics = new WHMapperStoreMetrics(_mockLogger.Object, _mockMeterFactory.Object, _mockConfiguration.Object);

        // Act
        metrics.DeleteJumpLogs(6);

        // Assert
        _mockCounter.Verify(c => c.Add(6), Times.Once);
        _mockUpDownCounter.Verify(c => c.Add(-6), Times.Once);
    }

    private void SetupValidMetrics()
    {
        _mockConfiguration.Setup(c => c["WHMapperStoreMeterName"]).Returns("test-meter");
        _mockMeterFactory.Setup(f => f.Create("test-meter")).Returns(_mockMeter.Object);
        _mockMeter.Setup(m => m.CreateCounter<int>(It.IsAny<string>(), null, null))
               .Returns(_mockCounter.Object);
        _mockMeter.Setup(m => m.CreateUpDownCounter<int>(It.IsAny<string>(), null, null))
               .Returns(_mockUpDownCounter.Object);
    }
}
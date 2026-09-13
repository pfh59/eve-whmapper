using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Repositories.WHActivityLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.WHActivityLogs;

namespace WHMapper.Tests.Services.WHActivityLogs;

public class WHActivityLogServiceTests
{
    private const int CHARACTER_ID = 2113720458;
    private const int MAP_ID = 7;
    private const int INSTANCE_ID = 3;

    private readonly Mock<IWHActivityLogRepository> _activityLogRepositoryMock = new();
    private readonly Mock<IWHMapRepository> _mapRepositoryMock = new();
    private readonly List<WHActivityLog> _storedActivities = new();

    public WHActivityLogServiceTests()
    {
        _mapRepositoryMock.Setup(r => r.GetInstanceIdAsync(MAP_ID)).ReturnsAsync(INSTANCE_ID);
        _activityLogRepositoryMock
            .Setup(r => r.CreateRange(It.IsAny<IEnumerable<WHActivityLog>>()))
            .Callback<IEnumerable<WHActivityLog>>(activities => _storedActivities.AddRange(activities))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task RecordAsync_WhenMapBelongsToInstance_StoresCharacterTypeMapAndInstance()
    {
        await CreateService().RecordAsync(CHARACTER_ID, WHActivityTypeIds.SystemOpened, MAP_ID);

        var activity = Assert.Single(_storedActivities);
        Assert.Equal(CHARACTER_ID, activity.CharacterId);
        Assert.Equal(WHActivityTypeIds.SystemOpened, activity.WHActivityTypeId);
        Assert.Equal(MAP_ID, activity.WHMapId);
        Assert.Equal(INSTANCE_ID, activity.WHInstanceId);
        Assert.Equal(DateTimeKind.Utc, activity.ActivityDate.Kind);
    }

    [Fact]
    public async Task RecordAsync_WhenOccurrencesIsThree_StoresThreeActivities()
    {
        await CreateService().RecordAsync(CHARACTER_ID, WHActivityTypeIds.SignatureCreated, MAP_ID, 3);

        Assert.Equal(3, _storedActivities.Count);
        Assert.All(_storedActivities, x => Assert.Equal(WHActivityTypeIds.SignatureCreated, x.WHActivityTypeId));
    }

    [Fact]
    public async Task RecordAsync_WhenOccurrencesIsZero_StoresNothing()
    {
        await CreateService().RecordAsync(CHARACTER_ID, WHActivityTypeIds.SignatureUpdated, MAP_ID, 0);

        _mapRepositoryMock.Verify(r => r.GetInstanceIdAsync(It.IsAny<int>()), Times.Never);
        _activityLogRepositoryMock.Verify(r => r.CreateRange(It.IsAny<IEnumerable<WHActivityLog>>()), Times.Never);
    }

    [Fact]
    public async Task RecordAsync_WhenMapIdIsInvalid_StoresNothing()
    {
        await CreateService().RecordAsync(CHARACTER_ID, WHActivityTypeIds.SystemOpened, 0);

        _activityLogRepositoryMock.Verify(r => r.CreateRange(It.IsAny<IEnumerable<WHActivityLog>>()), Times.Never);
    }

    [Fact]
    public async Task RecordAsync_WhenRepositoryThrows_DoesNotThrow()
    {
        _activityLogRepositoryMock
            .Setup(r => r.CreateRange(It.IsAny<IEnumerable<WHActivityLog>>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        var exception = await Record.ExceptionAsync(() => CreateService().RecordAsync(CHARACTER_ID, WHActivityTypeIds.SystemOpened, MAP_ID));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RecordSignatureImportAsync_RecordsCreatedAndChangedSignatures()
    {
        var importResult = new WHSignatureImportResult(true, 2, 1);

        await CreateService().RecordSignatureImportAsync(CHARACTER_ID, MAP_ID, importResult);

        Assert.Equal(2, _storedActivities.Count(x => x.WHActivityTypeId == WHActivityTypeIds.SignatureCreated));
        Assert.Equal(1, _storedActivities.Count(x => x.WHActivityTypeId == WHActivityTypeIds.SignatureUpdated));
    }

    [Fact]
    public void RetentionDays_WhenNotConfigured_IsDefault()
    {
        Assert.Equal(WHActivityLogService.DEFAULT_RETENTION_DAYS, CreateService().RetentionDays);
    }

    [Fact]
    public async Task PurgeExpiredAsync_WhenRetentionIsZero_DeletesNothing()
    {
        int deletedCount = await CreateService(retentionDays: 0).PurgeExpiredAsync();

        Assert.Equal(0, deletedCount);
        _activityLogRepositoryMock.Verify(r => r.DeleteOlderThanAsync(It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task PurgeExpiredAsync_WhenRetentionIsSet_DeletesOlderThanCutoff()
    {
        DateTime expectedCutoff = DateTime.UtcNow.AddDays(-90);
        _activityLogRepositoryMock
            .Setup(r => r.DeleteOlderThanAsync(It.Is<DateTime>(d => Math.Abs((d - expectedCutoff).TotalMinutes) < 1)))
            .ReturnsAsync(4);

        int deletedCount = await CreateService(retentionDays: 90).PurgeExpiredAsync();

        Assert.Equal(4, deletedCount);
    }

    private WHActivityLogService CreateService(int? retentionDays = null)
    {
        var settings = new Dictionary<string, string?>();
        if (retentionDays.HasValue)
            settings[WHActivityLogService.RETENTION_DAYS_CONFIG_KEY] = retentionDays.Value.ToString(CultureInfo.InvariantCulture);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new WHActivityLogService(
            _activityLogRepositoryMock.Object,
            _mapRepositoryMock.Object,
            configuration,
            NullLogger<WHActivityLogService>.Instance);
    }
}

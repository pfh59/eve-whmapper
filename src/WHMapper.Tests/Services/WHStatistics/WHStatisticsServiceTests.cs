using System.Globalization;
using AutoFixture.Xunit2;
using Moq;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.Statistics;
using WHMapper.Repositories.WHActivityLogs;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHStatistics;

namespace WHMapper.Tests.Services.WHStatistics;

public class WHStatisticsServiceTests
{
    private const int VIEWER_ID = 2113720458;
    private const int INSTANCE_ID = 1;
    private const int MAP_ID = 10;
    private const int OTHER_MAP_ID = 11;
    private const int PILOT_A = 1001;
    private const int PILOT_B = 1002;

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_WhenViewerHasNoAccessibleMap_ReturnsEmptyReports(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        instanceServiceMock.Setup(s => s.GetMapsAsync(INSTANCE_ID)).ReturnsAsync(CreateMaps(MAP_ID));
        accessHelperMock.Setup(h => h.IsEveMapperMapAccessAuthorized(VIEWER_ID, MAP_ID)).ReturnsAsync(false);

        var reports = await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null);

        AssertEveryPeriodEmpty(reports);
        VerifyCountsNeverRequested(repositoryMock);
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_WhenMapIsNotAccessible_ReturnsEmptyReports(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);

        var reports = await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, OTHER_MAP_ID);

        AssertEveryPeriodEmpty(reports);
        VerifyCountsNeverRequested(repositoryMock);
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_WhenMapIsSelected_CountsOnlyThatMap(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID, OTHER_MAP_ID);
        SetupCounts(repositoryMock);

        await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, OTHER_MAP_ID);

        // One count query per period, each restricted to the selected map.
        repositoryMock.Verify(r => r.GetCountsByCharacterAsync(
            INSTANCE_ID,
            It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(OTHER_MAP_ID)),
            It.IsAny<IReadOnlyCollection<int>>(),
            It.IsAny<DateTime?>()), Times.Exactly(Enum.GetValues<WHStatisticsPeriod>().Length));
        VerifyCountsRequestedOnlyFor(repositoryMock, OTHER_MAP_ID);
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_CountsCreatedAndUpdatedIndependently(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);
        SetupCounts(repositoryMock,
            new WHActivityCount(PILOT_A, WHActivityTypeIds.SignatureCreated, 2),
            new WHActivityCount(PILOT_B, WHActivityTypeIds.SignatureUpdated, 1));

        var report = (await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null))[WHStatisticsPeriod.AllTime];

        var pilotA = report.Probers.Single(x => x.CharacterId == PILOT_A);
        Assert.Equal((2, 0, 2), (pilotA.Created, pilotA.Updated, pilotA.Total));
        var pilotB = report.Probers.Single(x => x.CharacterId == PILOT_B);
        Assert.Equal((0, 1, 1), (pilotB.Created, pilotB.Updated, pilotB.Total));
        Assert.Empty(report.Explorers);
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_SortsProbersByTotalDescending(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);
        SetupCounts(repositoryMock,
            new WHActivityCount(PILOT_A, WHActivityTypeIds.SignatureCreated, 3),
            new WHActivityCount(PILOT_B, WHActivityTypeIds.SignatureCreated, 1),
            new WHActivityCount(PILOT_B, WHActivityTypeIds.SignatureUpdated, 4));

        var report = (await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null))[WHStatisticsPeriod.AllTime];

        Assert.Collection(report.Probers,
            first => Assert.Equal((1, PILOT_B, 5), (first.Rank, first.CharacterId, first.Total)),
            second => Assert.Equal((2, PILOT_A, 3), (second.Rank, second.CharacterId, second.Total)));
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_SortsExplorersBySystemsOpenedDescending(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);
        SetupCounts(repositoryMock,
            new WHActivityCount(PILOT_A, WHActivityTypeIds.SystemOpened, 2),
            new WHActivityCount(PILOT_B, WHActivityTypeIds.SystemOpened, 7));

        var report = (await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null))[WHStatisticsPeriod.AllTime];

        Assert.Collection(report.Explorers,
            first => Assert.Equal((1, PILOT_B, 7), (first.Rank, first.CharacterId, first.SystemsOpened)),
            second => Assert.Equal((2, PILOT_A, 2), (second.Rank, second.CharacterId, second.SystemsOpened)));
        Assert.Empty(report.Probers);
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_PassesTheCutoffOfEachPeriod(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);
        SetupCounts(repositoryMock);
        DateTime expectedWeekCutoff = DateTime.UtcNow.AddDays(-7);
        DateTime expectedMonthCutoff = DateTime.UtcNow.AddDays(-30);

        await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null);

        VerifyCountsRequestedWithCutoff(repositoryMock, d => d.HasValue && Math.Abs((d.Value - expectedWeekCutoff).TotalMinutes) < 1);
        VerifyCountsRequestedWithCutoff(repositoryMock, d => d.HasValue && Math.Abs((d.Value - expectedMonthCutoff).TotalMinutes) < 1);
        VerifyCountsRequestedWithCutoff(repositoryMock, d => !d.HasValue);
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_RanksEachPeriodFromItsOwnCounts(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);
        // PILOT_A leads over all time, PILOT_B leads over the last 7 days only.
        repositoryMock
            .Setup(r => r.GetCountsByCharacterAsync(INSTANCE_ID, It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<DateTime?>()))
            .ReturnsAsync((int _, IReadOnlyCollection<int> _, IReadOnlyCollection<int> _, DateTime? fromUtc) => !fromUtc.HasValue
                ? new[] { new WHActivityCount(PILOT_A, WHActivityTypeIds.SystemOpened, 9), new WHActivityCount(PILOT_B, WHActivityTypeIds.SystemOpened, 3) }
                : new[] { new WHActivityCount(PILOT_B, WHActivityTypeIds.SystemOpened, 3) });

        var reports = await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null);

        Assert.Equal(PILOT_B, Assert.Single(reports[WHStatisticsPeriod.Last7Days].Explorers).CharacterId);
        Assert.Equal(PILOT_B, Assert.Single(reports[WHStatisticsPeriod.Last30Days].Explorers).CharacterId);
        Assert.Collection(reports[WHStatisticsPeriod.AllTime].Explorers,
            first => Assert.Equal((1, PILOT_A, 9), (first.Rank, first.CharacterId, first.SystemsOpened)),
            second => Assert.Equal((2, PILOT_B, 3), (second.Rank, second.CharacterId, second.SystemsOpened)));
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_WhenMoreThanTenCharacters_ReturnsTopTenOfEachPeriod(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);
        var counts = Enumerable.Range(1, 60)
            .SelectMany(characterId => new[]
            {
                new WHActivityCount(characterId, WHActivityTypeIds.SignatureCreated, characterId),
                new WHActivityCount(characterId, WHActivityTypeIds.SystemOpened, characterId)
            })
            .ToArray();
        SetupCounts(repositoryMock, counts);

        var reports = await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null);

        Assert.Equal(10, WHStatisticsService.LEADERBOARD_SIZE);
        Assert.All(reports.Values, report =>
        {
            Assert.Equal(WHStatisticsService.LEADERBOARD_SIZE, report.Probers.Count);
            Assert.Equal(WHStatisticsService.LEADERBOARD_SIZE, report.Explorers.Count);
            Assert.Equal(60, report.Probers[0].CharacterId);
            Assert.Equal(51, report.Probers[^1].CharacterId);
            Assert.Equal(WHStatisticsService.LEADERBOARD_SIZE, report.Probers[^1].Rank);
        });
    }

    [Theory, AutoMoqData]
    public async Task GetLeaderboardsAsync_ResolvesCharacterNames(
        [Frozen] Mock<IWHActivityLogRepository> repositoryMock,
        [Frozen] Mock<IEveMapperAccessHelper> accessHelperMock,
        [Frozen] Mock<IEveMapperInstanceService> instanceServiceMock,
        [Frozen] Mock<IEveMapperService> eveMapperServiceMock,
        WHStatisticsService sut)
    {
        SetupAccessibleMaps(instanceServiceMock, accessHelperMock, MAP_ID);
        SetupCounts(repositoryMock,
            new WHActivityCount(PILOT_A, WHActivityTypeIds.SignatureCreated, 1),
            new WHActivityCount(PILOT_B, WHActivityTypeIds.SystemOpened, 1));
        eveMapperServiceMock.Setup(s => s.GetCharacter(PILOT_A)).ReturnsAsync(new CharacterEntity(PILOT_A, "Pilot A"));
        eveMapperServiceMock.Setup(s => s.GetCharacter(PILOT_B)).ThrowsAsync(new HttpRequestException("ESI unavailable"));

        var report = (await sut.GetLeaderboardsAsync(VIEWER_ID, INSTANCE_ID, null))[WHStatisticsPeriod.AllTime];

        Assert.Equal("Pilot A", Assert.Single(report.Probers).CharacterName);
        Assert.Equal(PILOT_B.ToString(CultureInfo.InvariantCulture), Assert.Single(report.Explorers).CharacterName);
        // Names are resolved once per character, not once per period.
        eveMapperServiceMock.Verify(s => s.GetCharacter(PILOT_A), Times.Once);
        eveMapperServiceMock.Verify(s => s.GetCharacter(PILOT_B), Times.Once);
    }

    private static List<WHMap> CreateMaps(params int[] mapIds)
    {
        return mapIds.Select(id => new WHMap($"Map {id}", INSTANCE_ID) { Id = id }).ToList();
    }

    private static void SetupAccessibleMaps(Mock<IEveMapperInstanceService> instanceServiceMock, Mock<IEveMapperAccessHelper> accessHelperMock, params int[] mapIds)
    {
        instanceServiceMock.Setup(s => s.GetMapsAsync(INSTANCE_ID)).ReturnsAsync(CreateMaps(mapIds));
        accessHelperMock.Setup(h => h.IsEveMapperMapAccessAuthorized(VIEWER_ID, It.IsAny<int>())).ReturnsAsync(true);
    }

    private static void SetupCounts(Mock<IWHActivityLogRepository> repositoryMock, params WHActivityCount[] counts)
    {
        repositoryMock
            .Setup(r => r.GetCountsByCharacterAsync(INSTANCE_ID, It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(counts);
    }

    private static void AssertEveryPeriodEmpty(IReadOnlyDictionary<WHStatisticsPeriod, WHStatisticsReport> reports)
    {
        Assert.Equal(Enum.GetValues<WHStatisticsPeriod>(), reports.Keys.Order());
        Assert.All(reports.Values, report =>
        {
            Assert.Empty(report.Probers);
            Assert.Empty(report.Explorers);
        });
    }

    private static void VerifyCountsRequestedOnlyFor(Mock<IWHActivityLogRepository> repositoryMock, int mapId)
    {
        repositoryMock.Verify(r => r.GetCountsByCharacterAsync(
            It.IsAny<int>(),
            It.Is<IReadOnlyCollection<int>>(ids => !(ids.Count == 1 && ids.Contains(mapId))),
            It.IsAny<IReadOnlyCollection<int>>(),
            It.IsAny<DateTime?>()), Times.Never);
    }

    private static void VerifyCountsRequestedWithCutoff(Mock<IWHActivityLogRepository> repositoryMock, System.Linq.Expressions.Expression<Func<DateTime?, bool>> cutoff)
    {
        repositoryMock.Verify(r => r.GetCountsByCharacterAsync(
            INSTANCE_ID,
            It.IsAny<IReadOnlyCollection<int>>(),
            It.IsAny<IReadOnlyCollection<int>>(),
            It.Is(cutoff)), Times.Once);
    }

    private static void VerifyCountsNeverRequested(Mock<IWHActivityLogRepository> repositoryMock)
    {
        repositoryMock.Verify(r => r.GetCountsByCharacterAsync(
            It.IsAny<int>(),
            It.IsAny<IReadOnlyCollection<int>>(),
            It.IsAny<IReadOnlyCollection<int>>(),
            It.IsAny<DateTime?>()), Times.Never);
    }
}

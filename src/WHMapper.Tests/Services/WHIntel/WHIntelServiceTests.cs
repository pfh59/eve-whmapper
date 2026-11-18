using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.Intel;
using WHMapper.Models.DTO.ZKillboard;
using WHMapper.Services.Cache;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHIntel;
using WHMapper.Services.ZKillboardAPI;

namespace WHMapper.Tests.Services.WHIntel;

public class WHIntelServiceTests
{
    private const int VIEWER_ID = 2113720458;
    private const int MAP_ID = 10;
    private const int SYSTEM_ID = KillmailBuilder.SYSTEM_ID;
    private const string SYSTEM_NAME = "J165153";

    private const int RESIDENT_CORP = 98000001;
    private const int VISITOR_CORP = 98000003;
    private const int HAC_TYPE = 12005;
    private const int HAC_GROUP = 358;

    private static readonly DateTime DAY = new(2026, 11, 10, 18, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IZKillboardAPIServices> _zKillboard = new();
    private readonly Mock<IWHIntelAiReporter> _aiReporter = new();
    private readonly Mock<IEveMapperAccessHelper> _accessHelper = new();
    private readonly Mock<IEveAPIServices> _eveApi = new();
    private readonly Mock<IUniverseServices> _universe = new();
    private readonly Mock<IEveMapperService> _eveMapperService = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly WHIntelService _sut;

    public WHIntelServiceTests()
    {
        _accessHelper.Setup(x => x.IsEveMapperMapAccessAuthorized(VIEWER_ID, MAP_ID)).ReturnsAsync(true);
        _eveApi.Setup(x => x.UniverseServices).Returns(_universe.Object);
        _universe.Setup(x => x.GetNames(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync((IReadOnlyCollection<int> ids) => Result<UniverseName[]>.Success(
                ids.Select(id => new UniverseName { Id = id, Name = $"Name {id}" }).ToArray()));
        _universe.Setup(x => x.GetType(HAC_TYPE))
            .ReturnsAsync(Result<WHMapper.Models.DTO.EveAPI.Universe.Type>.Success(
                new WHMapper.Models.DTO.EveAPI.Universe.Type { GroupId = HAC_GROUP }));
        _eveMapperService.Setup(x => x.GetGroup(HAC_GROUP))
            .ReturnsAsync(new GroupEntity(HAC_GROUP, "Heavy Assault Cruiser", [], 6));
        _zKillboard.Setup(x => x.GetSystemStatsAsync(SYSTEM_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ZKillStats());
        _aiReporter.Setup(x => x.IsEnabled).Returns(true);

        _sut = new WHIntelService(
            _zKillboard.Object,
            new WHIntelAnalyzer(),
            _aiReporter.Object,
            _accessHelper.Object,
            _eveApi.Object,
            _eveMapperService.Object,
            _cache.Object,
            NullLogger<WHIntelService>.Instance);
    }

    private void SetupKillmails(IReadOnlyList<ZKillmail>? killmails) =>
        _zKillboard.Setup(x => x.GetSystemKillmailsAsync(SYSTEM_ID, It.IsAny<CancellationToken>())).ReturnsAsync(killmails);

    private static List<ZKillmail> CreateKillmails(int count) => Enumerable.Range(0, count)
        .Select(i => new KillmailBuilder(DAY.AddMinutes(-i))
            .Victim(VISITOR_CORP)
            .Attacker(RESIDENT_CORP, HAC_TYPE, finalBlow: true)
            .Value(1_000_000)
            .Build())
        .ToList();

    [Fact]
    public async Task GetIntelAsync_WhenViewerCannotOpenMap_ReturnsUnauthorizedWithoutCallingZKillboard()
    {
        _accessHelper.Setup(x => x.IsEveMapperMapAccessAuthorized(VIEWER_ID, MAP_ID)).ReturnsAsync(false);

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(WHIntelStatus.Unauthorized, report.Status);
        _zKillboard.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetIntelAsync_WhenZKillboardFails_ReturnsUnavailableAndCachesNothing()
    {
        SetupKillmails(null);

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(WHIntelStatus.Unavailable, report.Status);
        _cache.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<WHIntelKillData>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public async Task GetIntelAsync_WhenSystemHasNoKill_ReturnsNoActivity()
    {
        SetupKillmails([]);

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(WHIntelStatus.NoActivity, report.Status);
        Assert.Empty(report.RecentKills);
    }

    [Fact]
    public async Task GetIntelAsync_ReturnsFiftyMostRecentKillsWithResolvedNames()
    {
        SetupKillmails(CreateKillmails(60));

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(WHIntelStatus.Available, report.Status);
        Assert.Equal(WHIntelService.RECENT_KILLS_COUNT, report.RecentKills.Count);
        Assert.Equal(report.RecentKills.OrderByDescending(x => x.KillTimeUtc), report.RecentKills);

        var kill = report.RecentKills[0];
        Assert.Equal($"Name {VISITOR_CORP}", report.GetName(kill.VictimCorporationId));
        Assert.Equal($"Name {RESIDENT_CORP}", report.GetName(kill.FinalBlowCorporationId));
        Assert.Equal(1, kill.AttackerCount);
        Assert.Equal($"https://zkillboard.com/kill/{kill.KillmailId}/", kill.ZKillboardUrl);

        Assert.NotNull(report.Analysis);
        Assert.Equal("Heavy Assault Cruiser", report.GetShipGroupName(HAC_TYPE));
    }

    [Fact]
    public async Task GetIntelAsync_CachesKillmailsForOneHourAndStatisticsForTenMinutes()
    {
        SetupKillmails(CreateKillmails(5));

        await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        _cache.Verify(x => x.Set($"WHMapper:Intel:Killmails:{SYSTEM_ID}", It.IsAny<WHIntelKillData>(), WHIntelService.KILLMAILS_CACHE_DURATION), Times.Once);
        _cache.Verify(x => x.Set($"WHMapper:Intel:Stats:{SYSTEM_ID}", It.IsAny<WHIntelTopLists>(), WHIntelService.STATS_CACHE_DURATION), Times.Once);
    }

    [Fact]
    public async Task GetIntelAsync_WhenDataIsCached_DoesNotCallZKillboard()
    {
        var cached = new WHIntelKillData(CreateKillmails(5), new Dictionary<int, string>(), new Dictionary<int, string>());
        _cache.Setup(x => x.Get<WHIntelKillData>($"WHMapper:Intel:Killmails:{SYSTEM_ID}")).ReturnsAsync(cached);
        _cache.Setup(x => x.Get<WHIntelTopLists>($"WHMapper:Intel:Stats:{SYSTEM_ID}")).ReturnsAsync(WHIntelTopLists.Empty);

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(WHIntelStatus.Available, report.Status);
        Assert.Equal(5, report.RecentKills.Count);
        _zKillboard.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetIntelAsync_MapsTopTenOfEachList()
    {
        SetupKillmails(CreateKillmails(5));
        var stats = new ZKillStats
        {
            TopLists =
            [
                CreateTopList(ZKillTopList.TYPE_CHARACTER, 12),
                CreateTopList(ZKillTopList.TYPE_CORPORATION, 3),
                CreateTopList(ZKillTopList.TYPE_ALLIANCE, 1)
            ]
        };
        _zKillboard.Setup(x => x.GetSystemStatsAsync(SYSTEM_ID, It.IsAny<CancellationToken>())).ReturnsAsync(stats);

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(WHIntelService.TOP_LIST_SIZE, report.TopLists.Characters.Count);
        Assert.Equal(Enumerable.Range(1, 10), report.TopLists.Characters.Select(x => x.Rank));
        Assert.Equal("character 0", report.TopLists.Characters[0].Name);
        Assert.Equal(3, report.TopLists.Corporations.Count);
        Assert.Single(report.TopLists.Alliances);
    }

    [Fact]
    public async Task GetIntelAsync_WhenStatisticsFail_ReturnsKillsWithEmptyTopLists()
    {
        SetupKillmails(CreateKillmails(5));
        _zKillboard.Setup(x => x.GetSystemStatsAsync(SYSTEM_ID, It.IsAny<CancellationToken>())).ReturnsAsync((ZKillStats?)null);

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(WHIntelStatus.Available, report.Status);
        Assert.Empty(report.TopLists.Characters);
    }

    [Fact]
    public async Task GetIntelAsync_WhenNameResolutionFails_KeepsIds()
    {
        SetupKillmails(CreateKillmails(5));
        _universe.Setup(x => x.GetNames(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(Result<UniverseName[]>.Failure("Invalid id"));

        var report = await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);

        Assert.Equal(VISITOR_CORP.ToString(), report.GetName(report.RecentKills[0].VictimCorporationId));
    }

    [Fact]
    public async Task GetAiReportAsync_WhenAiIsNotConfigured_ReturnsNotConfigured()
    {
        _aiReporter.Setup(x => x.IsEnabled).Returns(false);

        var aiReport = await _sut.GetAiReportAsync(VIEWER_ID, MAP_ID, await LoadReportAsync(5));

        Assert.Equal(WHIntelAiStatus.NotConfigured, aiReport.Status);
        VerifyAiNeverCalled();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task GetAiReportAsync_WhenNotEnoughKills_ReturnsNotEnoughDataWithoutAiCall(int killCount)
    {
        var aiReport = await _sut.GetAiReportAsync(VIEWER_ID, MAP_ID, await LoadReportAsync(killCount));

        Assert.Equal(WHIntelAiStatus.NotEnoughData, aiReport.Status);
        VerifyAiNeverCalled();
    }

    [Fact]
    public async Task GetAiReportAsync_WhenViewerCannotOpenMap_ReturnsUnavailableWithoutAiCall()
    {
        var report = await LoadReportAsync(5);
        _accessHelper.Setup(x => x.IsEveMapperMapAccessAuthorized(VIEWER_ID, MAP_ID)).ReturnsAsync(false);

        var aiReport = await _sut.GetAiReportAsync(VIEWER_ID, MAP_ID, report);

        Assert.Equal(WHIntelAiStatus.Unavailable, aiReport.Status);
        VerifyAiNeverCalled();
    }

    [Fact]
    public async Task GetAiReportAsync_GeneratesAndCachesReportForOneHour()
    {
        _aiReporter.Setup(x => x.GenerateAsync(It.IsAny<WHIntelReport>(), It.IsAny<CancellationToken>())).ReturnsAsync("Residents: Name 98000001");

        var aiReport = await _sut.GetAiReportAsync(VIEWER_ID, MAP_ID, await LoadReportAsync(5));

        Assert.Equal(WHIntelAiStatus.Generated, aiReport.Status);
        Assert.Equal("Residents: Name 98000001", aiReport.Text);
        Assert.NotNull(aiReport.GeneratedAtUtc);
        _cache.Verify(x => x.Set($"WHMapper:Intel:AiReport:{SYSTEM_ID}", aiReport, WHIntelService.AI_REPORT_CACHE_DURATION), Times.Once);
    }

    [Fact]
    public async Task GetAiReportAsync_WhenReportIsCached_ReusesItWithoutAiCall()
    {
        var cached = new WHIntelAiReport(WHIntelAiStatus.Generated, "Residents: cached", DateTime.UtcNow.AddMinutes(-10));
        _cache.Setup(x => x.Get<WHIntelAiReport>($"WHMapper:Intel:AiReport:{SYSTEM_ID}")).ReturnsAsync(cached);

        var aiReport = await _sut.GetAiReportAsync(VIEWER_ID, MAP_ID, await LoadReportAsync(5));

        Assert.Same(cached, aiReport);
        VerifyAiNeverCalled();
    }

    [Fact]
    public async Task GetAiReportAsync_WhenAiFails_ReturnsUnavailableAndCachesNothing()
    {
        _aiReporter.Setup(x => x.GenerateAsync(It.IsAny<WHIntelReport>(), It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        var aiReport = await _sut.GetAiReportAsync(VIEWER_ID, MAP_ID, await LoadReportAsync(5));

        Assert.Equal(WHIntelAiStatus.Unavailable, aiReport.Status);
        _cache.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<WHIntelAiReport>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    private async Task<WHIntelReport> LoadReportAsync(int killCount)
    {
        SetupKillmails(CreateKillmails(killCount));
        return await _sut.GetIntelAsync(VIEWER_ID, MAP_ID, SYSTEM_ID, SYSTEM_NAME);
    }

    private void VerifyAiNeverCalled() =>
        _aiReporter.Verify(x => x.GenerateAsync(It.IsAny<WHIntelReport>(), It.IsAny<CancellationToken>()), Times.Never);

    private static ZKillTopList CreateTopList(string type, int count) => new()
    {
        Type = type,
        Values = Enumerable.Range(0, count).Select(i => new ZKillTopEntry { Id = i + 1, Name = $"{type} {i}", Kills = 100 - i }).ToList()
    };
}

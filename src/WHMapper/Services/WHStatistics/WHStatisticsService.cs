using System.Globalization;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.Statistics;
using WHMapper.Repositories.WHActivityLogs;
using WHMapper.Services.EveMapper;

namespace WHMapper.Services.WHStatistics;

/// <summary>
/// Default implementation of <see cref="IWHStatisticsService"/>.
/// </summary>
public class WHStatisticsService : IWHStatisticsService
{
    /// <summary>
    /// Maximum number of characters returned per leaderboard.
    /// </summary>
    public const int LEADERBOARD_SIZE = 10;

    private static readonly int[] LEADERBOARD_ACTIVITY_TYPE_IDS =
    {
        WHActivityTypeIds.SignatureCreated,
        WHActivityTypeIds.SignatureUpdated,
        WHActivityTypeIds.SystemOpened
    };

    private readonly IWHActivityLogRepository _activityLogRepository;
    private readonly IEveMapperAccessHelper _accessHelper;
    private readonly IEveMapperInstanceService _instanceService;
    private readonly IEveMapperService _eveMapperService;
    private readonly ILogger<WHStatisticsService> _logger;

    public WHStatisticsService(
        IWHActivityLogRepository activityLogRepository,
        IEveMapperAccessHelper accessHelper,
        IEveMapperInstanceService instanceService,
        IEveMapperService eveMapperService,
        ILogger<WHStatisticsService> logger)
    {
        _activityLogRepository = activityLogRepository;
        _accessHelper = accessHelper;
        _instanceService = instanceService;
        _eveMapperService = eveMapperService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WHInstance>> GetAccessibleInstancesAsync(int viewerCharacterId)
    {
        var instances = new List<WHInstance>();
        foreach (int instanceId in await _accessHelper.GetAccessibleInstanceIdsAsync(viewerCharacterId))
        {
            var instance = await _instanceService.GetInstanceAsync(instanceId);
            if (instance != null)
                instances.Add(instance);
        }

        return instances.OrderBy(x => x.Name).ToList();
    }

    public async Task<IReadOnlyList<WHMap>> GetAccessibleMapsAsync(int viewerCharacterId, int instanceId)
    {
        var maps = await _instanceService.GetMapsAsync(instanceId);
        if (maps == null)
            return Array.Empty<WHMap>();

        var accessibleMaps = new List<WHMap>();
        foreach (var map in maps)
        {
            // Checks instance access and per-map access restrictions.
            if (await _accessHelper.IsEveMapperMapAccessAuthorized(viewerCharacterId, map.Id))
                accessibleMaps.Add(map);
        }

        return accessibleMaps.OrderBy(x => x.Name).ToList();
    }

    public async Task<IReadOnlyDictionary<WHStatisticsPeriod, WHStatisticsReport>> GetLeaderboardsAsync(int viewerCharacterId, int instanceId, int? mapId)
    {
        var periods = Enum.GetValues<WHStatisticsPeriod>();

        // Authorization boundary: only activities on maps the viewer can open are counted.
        var mapIds = (await GetAccessibleMapsAsync(viewerCharacterId, instanceId)).Select(x => x.Id).ToList();
        if (mapId.HasValue)
            mapIds = mapIds.Contains(mapId.Value) ? new List<int> { mapId.Value } : new List<int>();

        if (mapIds.Count == 0)
            return periods.ToDictionary(x => x, _ => WHStatisticsReport.Empty);

        // Same reference date for every period, so the three leaderboards are consistent with each other.
        DateTime utcNow = DateTime.UtcNow;
        var rankings = new Dictionary<WHStatisticsPeriod, (List<(int CharacterId, int Created, int Updated)> Probers, List<WHActivityCount> Explorers)>();
        foreach (var period in periods)
        {
            var counts = await _activityLogRepository.GetCountsByCharacterAsync(
                instanceId, mapIds, LEADERBOARD_ACTIVITY_TYPE_IDS, GetPeriodStartUtc(period, utcNow));
            rankings[period] = RankCharacters(counts);
        }

        var names = await ResolveCharacterNamesAsync(rankings.Values
            .SelectMany(x => x.Probers.Select(p => p.CharacterId).Concat(x.Explorers.Select(e => e.CharacterId)))
            .Distinct());

        return rankings.ToDictionary(
            x => x.Key,
            x => new WHStatisticsReport(
                x.Value.Probers.Select((p, index) => new WHProberRank(index + 1, p.CharacterId, names[p.CharacterId], p.Created, p.Updated, p.Created + p.Updated)).ToList(),
                x.Value.Explorers.Select((e, index) => new WHExplorerRank(index + 1, e.CharacterId, names[e.CharacterId], e.Count)).ToList()));
    }

    /// <summary>
    /// Sorts the counts of one period into the first <see cref="LEADERBOARD_SIZE"/> probers and explorers.
    /// </summary>
    private static (List<(int CharacterId, int Created, int Updated)> Probers, List<WHActivityCount> Explorers) RankCharacters(IReadOnlyList<WHActivityCount> counts)
    {
        var probers = counts
            .Where(x => x.ActivityTypeId == WHActivityTypeIds.SignatureCreated || x.ActivityTypeId == WHActivityTypeIds.SignatureUpdated)
            .GroupBy(x => x.CharacterId)
            .Select(g => (
                CharacterId: g.Key,
                Created: g.Where(x => x.ActivityTypeId == WHActivityTypeIds.SignatureCreated).Sum(x => x.Count),
                Updated: g.Where(x => x.ActivityTypeId == WHActivityTypeIds.SignatureUpdated).Sum(x => x.Count)))
            .Where(x => x.Created + x.Updated > 0)
            .OrderByDescending(x => x.Created + x.Updated)
            .ThenByDescending(x => x.Created)
            .ThenBy(x => x.CharacterId)
            .Take(LEADERBOARD_SIZE)
            .ToList();

        var explorers = counts
            .Where(x => x.ActivityTypeId == WHActivityTypeIds.SystemOpened && x.Count > 0)
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.CharacterId)
            .Take(LEADERBOARD_SIZE)
            .ToList();

        return (probers, explorers);
    }

    /// <summary>
    /// Returns the inclusive start of a period, or null when the period has no lower bound.
    /// </summary>
    private static DateTime? GetPeriodStartUtc(WHStatisticsPeriod period, DateTime utcNow) => period switch
    {
        WHStatisticsPeriod.Last7Days => utcNow.AddDays(-7),
        WHStatisticsPeriod.Last30Days => utcNow.AddDays(-30),
        WHStatisticsPeriod.AllTime => null,
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Unsupported statistics period")
    };

    /// <summary>
    /// Resolves the current name of each character, falling back to the character id when the name is unavailable.
    /// </summary>
    private async Task<Dictionary<int, string>> ResolveCharacterNamesAsync(IEnumerable<int> characterIds)
    {
        var names = new Dictionary<int, string>();

        // Sequential on purpose: lookups are cached, and scoped services must not be used concurrently.
        foreach (int characterId in characterIds)
        {
            string? name = null;
            try
            {
                name = (await _eveMapperService.GetCharacter(characterId))?.Name;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible to resolve the name of character {CharacterId}", characterId);
            }

            names[characterId] = string.IsNullOrWhiteSpace(name)
                ? characterId.ToString(CultureInfo.InvariantCulture)
                : name;
        }

        return names;
    }
}

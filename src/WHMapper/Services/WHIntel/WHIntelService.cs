using System.Globalization;
using WHMapper.Models.DTO.Intel;
using WHMapper.Models.DTO.ZKillboard;
using WHMapper.Services.Cache;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.EveMapper;
using WHMapper.Services.ZKillboardAPI;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Default implementation of <see cref="IWHIntelService"/>.
/// </summary>
public class WHIntelService : IWHIntelService
{
    public const int RECENT_KILLS_COUNT = 50;
    public const int TOP_LIST_SIZE = 10;

    // Durations follow zKillboard's caching guidance: killmail search 1 hour, statistics 10 minutes.
    public static readonly TimeSpan KILLMAILS_CACHE_DURATION = TimeSpan.FromHours(1);
    public static readonly TimeSpan STATS_CACHE_DURATION = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan AI_REPORT_CACHE_DURATION = TimeSpan.FromHours(1);
    private static readonly TimeSpan SHIP_GROUP_CACHE_DURATION = TimeSpan.FromDays(7);

    private const string KILLMAILS_CACHE_KEY = "WHMapper:Intel:Killmails:{0}";
    private const string STATS_CACHE_KEY = "WHMapper:Intel:Stats:{0}";
    private const string AI_REPORT_CACHE_KEY = "WHMapper:Intel:AiReport:{0}";
    private const string SHIP_GROUP_CACHE_KEY = "WHMapper:Intel:ShipGroup:{0}";

    private readonly IZKillboardAPIServices _zKillboard;
    private readonly IWHIntelAnalyzer _analyzer;
    private readonly IWHIntelAiReporter _aiReporter;
    private readonly IEveMapperAccessHelper _accessHelper;
    private readonly IEveAPIServices _eveApi;
    private readonly IEveMapperService _eveMapperService;
    private readonly ICacheService _cache;
    private readonly ILogger<WHIntelService> _logger;

    public WHIntelService(
        IZKillboardAPIServices zKillboard,
        IWHIntelAnalyzer analyzer,
        IWHIntelAiReporter aiReporter,
        IEveMapperAccessHelper accessHelper,
        IEveAPIServices eveApi,
        IEveMapperService eveMapperService,
        ICacheService cache,
        ILogger<WHIntelService> logger)
    {
        _zKillboard = zKillboard;
        _analyzer = analyzer;
        _aiReporter = aiReporter;
        _accessHelper = accessHelper;
        _eveApi = eveApi;
        _eveMapperService = eveMapperService;
        _cache = cache;
        _logger = logger;
    }

    public bool IsAiEnabled => _aiReporter.IsEnabled;

    public async Task<WHIntelReport> GetIntelAsync(int viewerCharacterId, int mapId, int solarSystemId, string systemName, CancellationToken cancellationToken = default)
    {
        if (!await _accessHelper.IsEveMapperMapAccessAuthorized(viewerCharacterId, mapId))
        {
            _logger.LogWarning("Character {CharacterId} is not allowed to read the intel of map {MapId}", viewerCharacterId, mapId);
            return WHIntelReport.Empty(WHIntelStatus.Unauthorized, solarSystemId, systemName);
        }

        var killData = await GetKillDataAsync(solarSystemId, cancellationToken);
        if (killData == null)
            return WHIntelReport.Empty(WHIntelStatus.Unavailable, solarSystemId, systemName);

        if (killData.Killmails.Count == 0)
            return WHIntelReport.Empty(WHIntelStatus.NoActivity, solarSystemId, systemName);

        var topLists = await GetTopListsAsync(solarSystemId, cancellationToken);

        return new WHIntelReport(
            WHIntelStatus.Available,
            solarSystemId,
            systemName,
            killData.Killmails.Take(RECENT_KILLS_COUNT).Select(ToIntelKill).ToList(),
            topLists,
            _analyzer.Analyze(killData.Killmails),
            killData.Names,
            killData.ShipGroupNames);
    }

    public async Task<WHIntelAiReport> GetAiReportAsync(int viewerCharacterId, int mapId, WHIntelReport report, CancellationToken cancellationToken = default)
    {
        if (!_aiReporter.IsEnabled)
            return WHIntelAiReport.WithStatus(WHIntelAiStatus.NotConfigured);

        if (report.Status != WHIntelStatus.Available || report.Analysis == null)
            return WHIntelAiReport.WithStatus(WHIntelAiStatus.NotEnoughData);

        if (!await _accessHelper.IsEveMapperMapAccessAuthorized(viewerCharacterId, mapId))
        {
            _logger.LogWarning("Character {CharacterId} is not allowed to read the intel of map {MapId}", viewerCharacterId, mapId);
            return WHIntelAiReport.WithStatus(WHIntelAiStatus.Unavailable);
        }

        string cacheKey = FormatKey(AI_REPORT_CACHE_KEY, report.SolarSystemId);
        var cached = await _cache.Get<WHIntelAiReport>(cacheKey);
        if (cached != null)
            return cached;

        string? text = await _aiReporter.GenerateAsync(report, cancellationToken);
        if (text == null)
            return WHIntelAiReport.WithStatus(WHIntelAiStatus.Unavailable);

        var aiReport = new WHIntelAiReport(WHIntelAiStatus.Generated, text, DateTime.UtcNow);
        await _cache.Set(cacheKey, aiReport, AI_REPORT_CACHE_DURATION);
        return aiReport;
    }

    /// <summary>
    /// Killmails and resolved names of a system, from the cache or zKillboard; null when zKillboard fails.
    /// </summary>
    private async Task<WHIntelKillData?> GetKillDataAsync(int solarSystemId, CancellationToken cancellationToken)
    {
        string cacheKey = FormatKey(KILLMAILS_CACHE_KEY, solarSystemId);
        var cached = await _cache.Get<WHIntelKillData>(cacheKey);
        if (cached != null)
            return cached;

        var killmails = await _zKillboard.GetSystemKillmailsAsync(solarSystemId, cancellationToken);
        if (killmails == null)
            return null;

        var analysis = _analyzer.Analyze(killmails);
        var hullIds = analysis?.FleetHulls.Select(x => x.ShipTypeId).ToList() ?? [];
        var names = await ResolveNamesAsync(CollectReferencedIds(killmails, analysis), cancellationToken);
        var shipGroupNames = await ResolveShipGroupNamesAsync(hullIds);

        var killData = new WHIntelKillData(killmails, names, shipGroupNames);
        await _cache.Set(cacheKey, killData, KILLMAILS_CACHE_DURATION);
        return killData;
    }

    /// <summary>
    /// zKillboard top lists of a system, from the cache or zKillboard; empty when zKillboard fails.
    /// </summary>
    private async Task<WHIntelTopLists> GetTopListsAsync(int solarSystemId, CancellationToken cancellationToken)
    {
        string cacheKey = FormatKey(STATS_CACHE_KEY, solarSystemId);
        var cached = await _cache.Get<WHIntelTopLists>(cacheKey);
        if (cached != null)
            return cached;

        var stats = await _zKillboard.GetSystemStatsAsync(solarSystemId, cancellationToken);
        if (stats == null)
            return WHIntelTopLists.Empty;

        var topLists = new WHIntelTopLists(
            ToTopEntries(stats, ZKillTopList.TYPE_CHARACTER),
            ToTopEntries(stats, ZKillTopList.TYPE_CORPORATION),
            ToTopEntries(stats, ZKillTopList.TYPE_ALLIANCE));

        await _cache.Set(cacheKey, topLists, STATS_CACHE_DURATION);
        return topLists;
    }

    private static IReadOnlyList<WHIntelTopEntry> ToTopEntries(ZKillStats stats, string type) =>
        (stats.TopLists.FirstOrDefault(x => x.Type == type)?.Values ?? [])
            .Take(TOP_LIST_SIZE)
            .Select((x, index) => new WHIntelTopEntry(index + 1, x.Id, x.Name, x.Kills))
            .ToList();

    /// <summary>
    /// Ids displayed by the dialog and sent to the AI: the recent kills, the residents and the fleet hulls.
    /// </summary>
    private static HashSet<int> CollectReferencedIds(IReadOnlyList<ZKillmail> killmails, WHIntelAnalysis? analysis)
    {
        var ids = new HashSet<int>();

        void Add(int? id)
        {
            if (id is > 0)
                ids.Add(id.Value);
        }

        foreach (var killmail in killmails.Take(RECENT_KILLS_COUNT))
        {
            Add(killmail.Victim.CharacterId);
            Add(killmail.Victim.CorporationId);
            Add(killmail.Victim.AllianceId);
            Add(killmail.Victim.ShipTypeId);
            Add(killmail.Attackers.FirstOrDefault(x => x.FinalBlow)?.CorporationId);
        }

        foreach (var resident in analysis?.Residents ?? [])
        {
            Add(resident.CorporationId);
            Add(resident.AllianceId);
        }

        foreach (var hull in analysis?.FleetHulls ?? [])
            Add(hull.ShipTypeId);

        return ids;
    }

    /// <summary>
    /// Resolves names in bulk through ESI; unresolved ids are left out and displayed as numbers.
    /// </summary>
    private async Task<Dictionary<int, string>> ResolveNamesAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        var names = new Dictionary<int, string>();
        foreach (var chunk in ids.Chunk(UniverseServices.MAX_NAMES_PER_REQUEST))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _eveApi.UniverseServices.GetNames(chunk);
            if (!result.IsSuccess || result.Data == null)
            {
                _logger.LogWarning("Impossible to resolve {Count} intel names: {Error}", chunk.Length, result.ErrorMessage);
                continue;
            }

            foreach (var name in result.Data)
                names[name.Id] = name.Name;
        }

        return names;
    }

    /// <summary>
    /// Group names of the given ship types; types whose group cannot be resolved are left out.
    /// </summary>
    private async Task<Dictionary<int, string>> ResolveShipGroupNamesAsync(IReadOnlyList<int> shipTypeIds)
    {
        var groupNames = new Dictionary<int, string>();

        // Sequential on purpose: scoped services must not be used concurrently.
        foreach (int shipTypeId in shipTypeIds)
        {
            string cacheKey = FormatKey(SHIP_GROUP_CACHE_KEY, shipTypeId);
            string? groupName = await _cache.Get<string>(cacheKey);
            if (groupName == null)
            {
                var type = await _eveApi.UniverseServices.GetType(shipTypeId);
                if (type.IsSuccess && type.Data != null)
                    groupName = (await _eveMapperService.GetGroup(type.Data.GroupId))?.Name;

                if (groupName != null)
                    await _cache.Set(cacheKey, groupName, SHIP_GROUP_CACHE_DURATION);
            }

            if (groupName != null)
                groupNames[shipTypeId] = groupName;
        }

        return groupNames;
    }

    private static WHIntelKill ToIntelKill(ZKillmail killmail) => new(
        killmail.KillmailId,
        killmail.KillmailTime.ToUniversalTime(),
        killmail.Victim.ShipTypeId,
        killmail.Victim.CharacterId,
        killmail.Victim.CorporationId,
        killmail.Victim.AllianceId,
        killmail.Attackers.Count,
        killmail.Attackers.FirstOrDefault(x => x.FinalBlow)?.CorporationId,
        killmail.Zkb.TotalValue,
        killmail.Zkb.Npc,
        killmail.Zkb.Solo);

    private static string FormatKey(string format, int id) => string.Format(CultureInfo.InvariantCulture, format, id);
}

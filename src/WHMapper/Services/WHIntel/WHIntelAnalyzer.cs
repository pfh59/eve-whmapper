using WHMapper.Models.DTO.Intel;
using WHMapper.Models.DTO.ZKillboard;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Default implementation of <see cref="IWHIntelAnalyzer"/>.
/// </summary>
public class WHIntelAnalyzer : IWHIntelAnalyzer
{
    public const int MIN_KILLS_FOR_ANALYSIS = 5;
    public const int MAX_RESIDENTS = 5;
    public const int MAX_FLEET_HULLS = 8;

    /// <summary>
    /// Residents whose hulls describe the fleet composition.
    /// </summary>
    private const int FLEET_RESIDENTS = 3;

    /// <summary>
    /// At or above this NPC kill share the system is PVE; at or below <see cref="PVP_MAX_NPC_SHARE"/> it is PVP.
    /// </summary>
    private const double PVE_MIN_NPC_SHARE = 0.5;
    private const double PVP_MAX_NPC_SHARE = 0.2;

    private const int STRUCTURE_CATEGORY_ID = 65;

    // Residency score weights: owning structures is the strongest sign, then losing ships at home.
    private const int STRUCTURE_LOSS_WEIGHT = 5;
    private const int SHIP_LOSS_WEIGHT = 2;
    private const int KILL_WEIGHT = 1;
    private const int ACTIVE_DAY_WEIGHT = 1;

    // NPC corporations use ids 1,000,000 to 1,999,999.
    private const int NPC_CORPORATION_MIN_ID = 1_000_000;
    private const int NPC_CORPORATION_MAX_ID = 1_999_999;

    private const int CAPSULE_TYPE_ID = 670;
    private const int GENOLUTION_CAPSULE_TYPE_ID = 33328;

    // Prime-time windows in UTC hours: EU 14:00-22:59, AU/CN 06:00-13:59, US the remaining hours.
    public const int EU_FIRST_HOUR = 14;
    public const int EU_LAST_HOUR = 22;
    public const int AUCN_FIRST_HOUR = 6;
    public const int AUCN_LAST_HOUR = 13;

    private const int HOURS_PER_DAY = 24;

    public WHIntelAnalysis? Analyze(IReadOnlyList<ZKillmail> killmails)
    {
        if (killmails.Count < MIN_KILLS_FOR_ANALYSIS)
            return null;

        var residents = RankResidents(killmails);
        var killsPerHour = CountKillsPerUtcHour(killmails);
        var (hulls, fromResidents, averageGangSize) = DescribeFleet(killmails, residents);

        double npcShare = (double)killmails.Count(x => x.Zkb.Npc) / killmails.Count;
        double soloShare = (double)killmails.Count(x => x.Zkb.Solo) / killmails.Count;

        return new WHIntelAnalysis(
            killmails.Count,
            residents,
            GetProfile(npcShare),
            npcShare,
            soloShare,
            killsPerHour,
            GetTimezone(killsPerHour),
            hulls,
            fromResidents,
            averageGangSize);
    }

    private static WHIntelActivityProfile GetProfile(double npcShare) => npcShare switch
    {
        >= PVE_MIN_NPC_SHARE => WHIntelActivityProfile.PVE,
        <= PVP_MAX_NPC_SHARE => WHIntelActivityProfile.PVP,
        _ => WHIntelActivityProfile.Mixed
    };

    private static IReadOnlyList<WHIntelResident> RankResidents(IReadOnlyList<ZKillmail> killmails)
    {
        var stats = new Dictionary<int, ResidentStats>();

        ResidentStats GetStats(int corporationId)
        {
            if (!stats.TryGetValue(corporationId, out var value))
            {
                value = new ResidentStats();
                stats[corporationId] = value;
            }
            return value;
        }

        // Killmails are newest first, so the first alliance seen is the current one.
        foreach (var killmail in killmails)
        {
            var day = DateOnly.FromDateTime(killmail.KillmailTime);

            if (killmail.Victim.CorporationId is int victimCorporationId && IsPlayerCorporation(victimCorporationId))
            {
                var victim = GetStats(victimCorporationId);
                victim.AllianceId ??= killmail.Victim.AllianceId;
                victim.Days.Add(day);
                if (killmail.Zkb.VictimCategoryId == STRUCTURE_CATEGORY_ID)
                    victim.StructureLosses++;
                else
                    victim.ShipLosses++;
            }

            foreach (var attacker in killmail.Attackers
                .Where(x => x.CorporationId is int id && IsPlayerCorporation(id))
                .GroupBy(x => x.CorporationId!.Value))
            {
                var corporation = GetStats(attacker.Key);
                corporation.AllianceId ??= attacker.First().AllianceId;
                corporation.Days.Add(day);
                corporation.Kills++;
            }
        }

        return stats
            .Select(x => new WHIntelResident(
                x.Key,
                x.Value.AllianceId,
                x.Value.StructureLosses,
                x.Value.ShipLosses,
                x.Value.Kills,
                x.Value.Days.Count,
                x.Value.StructureLosses * STRUCTURE_LOSS_WEIGHT
                    + x.Value.ShipLosses * SHIP_LOSS_WEIGHT
                    + x.Value.Kills * KILL_WEIGHT
                    + x.Value.Days.Count * ACTIVE_DAY_WEIGHT))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.CorporationId)
            .Take(MAX_RESIDENTS)
            .ToList();
    }

    /// <summary>
    /// Hulls of the top residents' attackers; falls back to every player attacker when residents never attack.
    /// </summary>
    private static (IReadOnlyList<WHIntelHullUsage> Hulls, bool FromResidents, double AverageGangSize) DescribeFleet(
        IReadOnlyList<ZKillmail> killmails, IReadOnlyList<WHIntelResident> residents)
    {
        var residentIds = residents
            .Where(x => x.Kills > 0)
            .Take(FLEET_RESIDENTS)
            .Select(x => x.CorporationId)
            .ToHashSet();

        bool fromResidents = residentIds.Count > 0;
        Func<ZKillmailAttacker, bool> isFleetPilot = fromResidents
            ? x => x.CorporationId is int id && residentIds.Contains(id)
            : x => x.CharacterId.HasValue;

        var fleetKills = killmails
            .Select(x => x.Attackers.Where(isFleetPilot).ToList())
            .Where(x => x.Count > 0)
            .ToList();

        var hulls = fleetKills
            .SelectMany(x => x)
            .Where(x => x.ShipTypeId is int typeId && typeId > 0 && !IsCapsule(typeId))
            .GroupBy(x => x.ShipTypeId!.Value)
            .Select(x => new WHIntelHullUsage(x.Key, x.Count()))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.ShipTypeId)
            .Take(MAX_FLEET_HULLS)
            .ToList();

        double averageGangSize = fleetKills.Count == 0 ? 0 : fleetKills.Average(x => x.Count);
        return (hulls, fromResidents, averageGangSize);
    }

    private static int[] CountKillsPerUtcHour(IReadOnlyList<ZKillmail> killmails)
    {
        var counts = new int[HOURS_PER_DAY];
        foreach (var killmail in killmails)
            counts[killmail.KillmailTime.ToUniversalTime().Hour]++;
        return counts;
    }

    private static WHIntelTimezone GetTimezone(IReadOnlyList<int> killsPerHour)
    {
        var totals = new Dictionary<WHIntelTimezone, int>
        {
            [WHIntelTimezone.EU] = 0,
            [WHIntelTimezone.US] = 0,
            [WHIntelTimezone.AUCN] = 0
        };

        for (int hour = 0; hour < killsPerHour.Count; hour++)
            totals[GetTimezoneOfHour(hour)] += killsPerHour[hour];

        var best = totals.OrderByDescending(x => x.Value).First();
        return best.Value == 0 ? WHIntelTimezone.Unknown : best.Key;
    }

    private static WHIntelTimezone GetTimezoneOfHour(int hour) => hour switch
    {
        >= EU_FIRST_HOUR and <= EU_LAST_HOUR => WHIntelTimezone.EU,
        >= AUCN_FIRST_HOUR and <= AUCN_LAST_HOUR => WHIntelTimezone.AUCN,
        _ => WHIntelTimezone.US
    };

    private static bool IsPlayerCorporation(int corporationId) =>
        corporationId is < NPC_CORPORATION_MIN_ID or > NPC_CORPORATION_MAX_ID;

    private static bool IsCapsule(int shipTypeId) =>
        shipTypeId is CAPSULE_TYPE_ID or GENOLUTION_CAPSULE_TYPE_ID;

    private sealed class ResidentStats
    {
        public int? AllianceId { get; set; }
        public int StructureLosses { get; set; }
        public int ShipLosses { get; set; }
        public int Kills { get; set; }
        public HashSet<DateOnly> Days { get; } = new();
    }
}

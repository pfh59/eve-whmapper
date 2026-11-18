using WHMapper.Models.DTO.Intel;
using WHMapper.Models.DTO.ZKillboard;
using WHMapper.Services.WHIntel;

namespace WHMapper.Tests.Services.WHIntel;

public class WHIntelAnalyzerTests
{
    private const int RESIDENT_CORP = 98000001;
    private const int RESIDENT_ALLIANCE = 99000001;
    private const int ROAMER_CORP = 98000002;
    private const int VISITOR_CORP = 98000003;
    private const int NPC_CORP = 1000125;

    private const int HAC_TYPE = 12005;
    private const int LOGI_TYPE = 11985;
    private const int RATTING_TYPE = 17738;
    private const int CAPSULE_TYPE = 670;

    private static readonly DateTime DAY = new(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

    private readonly WHIntelAnalyzer _sut = new();

    [Fact]
    public void Analyze_WhenFewerThanMinimumKills_ReturnsNull()
    {
        var killmails = Enumerable.Range(0, WHIntelAnalyzer.MIN_KILLS_FOR_ANALYSIS - 1)
            .Select(i => new KillmailBuilder(DAY.AddHours(i)).Victim(VISITOR_CORP).Attacker(RESIDENT_CORP, HAC_TYPE).Build())
            .ToList();

        Assert.Null(_sut.Analyze(killmails));
    }

    [Fact]
    public void Analyze_RanksCorporationLosingStructureAndShipsAboveRoamer()
    {
        var killmails = new List<ZKillmail>
        {
            new KillmailBuilder(DAY.AddDays(2)).Victim(RESIDENT_CORP, RESIDENT_ALLIANCE, shipTypeId: 35833, characterId: null, categoryId: KillmailBuilder.STRUCTURE_CATEGORY)
                .Attacker(VISITOR_CORP, HAC_TYPE).Build(),
            new KillmailBuilder(DAY.AddDays(1)).Victim(VISITOR_CORP).Attacker(ROAMER_CORP, HAC_TYPE).Build()
        };
        killmails.AddRange(Enumerable.Range(0, 6).Select(i =>
            new KillmailBuilder(DAY.AddHours(i)).Victim(RESIDENT_CORP, RESIDENT_ALLIANCE, RATTING_TYPE).Npc().Attacker(NPC_CORP, 0, characterId: null).Build()));

        var analysis = _sut.Analyze(killmails)!;

        var resident = analysis.Residents[0];
        Assert.Equal(RESIDENT_CORP, resident.CorporationId);
        Assert.Equal(RESIDENT_ALLIANCE, resident.AllianceId);
        Assert.Equal(1, resident.StructureLosses);
        Assert.Equal(6, resident.ShipLosses);
        Assert.Equal(2, resident.ActiveDays);

        var ranking = analysis.Residents.Select(x => x.CorporationId).ToList();
        Assert.Contains(ROAMER_CORP, ranking);
        Assert.True(ranking.IndexOf(ROAMER_CORP) > ranking.IndexOf(RESIDENT_CORP));
    }

    [Fact]
    public void Analyze_IgnoresNpcCorporations()
    {
        var killmails = Enumerable.Range(0, 6)
            .Select(i => new KillmailBuilder(DAY.AddHours(i)).Victim(RESIDENT_CORP).Npc().Attacker(NPC_CORP, 0, characterId: null).Build())
            .ToList();

        var analysis = _sut.Analyze(killmails)!;

        Assert.DoesNotContain(analysis.Residents, x => x.CorporationId == NPC_CORP);
    }

    [Theory]
    [InlineData(7, WHIntelActivityProfile.PVE)]
    [InlineData(5, WHIntelActivityProfile.PVE)]
    [InlineData(3, WHIntelActivityProfile.Mixed)]
    [InlineData(1, WHIntelActivityProfile.PVP)]
    public void Analyze_ClassifiesActivityProfileFromNpcKillShare(int npcKills, WHIntelActivityProfile expected)
    {
        var killmails = Enumerable.Range(0, 10).Select(i =>
        {
            var builder = new KillmailBuilder(DAY.AddHours(i)).Victim(VISITOR_CORP).Attacker(RESIDENT_CORP, HAC_TYPE);
            return (i < npcKills ? builder.Npc() : builder).Build();
        }).ToList();

        var analysis = _sut.Analyze(killmails)!;

        Assert.Equal(expected, analysis.Profile);
        Assert.Equal(npcKills / 10d, analysis.NpcKillShare, 3);
    }

    [Fact]
    public void Analyze_ComputesSoloShare()
    {
        var killmails = Enumerable.Range(0, 5).Select(i =>
        {
            var builder = new KillmailBuilder(DAY.AddHours(i)).Victim(VISITOR_CORP).Attacker(RESIDENT_CORP, HAC_TYPE);
            return (i < 2 ? builder.Solo() : builder).Build();
        }).ToList();

        Assert.Equal(0.4, _sut.Analyze(killmails)!.SoloKillShare, 3);
    }

    [Theory]
    [InlineData(18, WHIntelTimezone.EU)]
    [InlineData(1, WHIntelTimezone.US)]
    [InlineData(23, WHIntelTimezone.US)]
    [InlineData(8, WHIntelTimezone.AUCN)]
    public void Analyze_EstimatesTimezoneFromKillHours(int hour, WHIntelTimezone expected)
    {
        var killmails = Enumerable.Range(0, 5)
            .Select(i => new KillmailBuilder(DAY.AddDays(i).AddHours(hour)).Victim(VISITOR_CORP).Attacker(RESIDENT_CORP, HAC_TYPE).Build())
            .ToList();

        var analysis = _sut.Analyze(killmails)!;

        Assert.Equal(expected, analysis.Timezone);
        Assert.Equal(24, analysis.KillsPerUtcHour.Count);
        Assert.Equal(5, analysis.KillsPerUtcHour[hour]);
    }

    [Fact]
    public void Analyze_DescribesResidentFleetWithoutCapsules()
    {
        // Residents kill visitors with 2 HACs and 1 Logistics cruiser; a capsule and the visitor's ship are ignored.
        var killmails = Enumerable.Range(0, 5)
            .Select(i => new KillmailBuilder(DAY.AddHours(18).AddDays(i))
                .Victim(VISITOR_CORP)
                .Attacker(RESIDENT_CORP, HAC_TYPE, finalBlow: true)
                .Attacker(RESIDENT_CORP, HAC_TYPE)
                .Attacker(RESIDENT_CORP, LOGI_TYPE)
                .Attacker(RESIDENT_CORP, CAPSULE_TYPE)
                .Build())
            .ToList();

        var analysis = _sut.Analyze(killmails)!;

        Assert.True(analysis.FleetFromResidents);
        Assert.Equal([HAC_TYPE, LOGI_TYPE], analysis.FleetHulls.Select(x => x.ShipTypeId));
        Assert.Equal([10, 5], analysis.FleetHulls.Select(x => x.Count));
        Assert.Equal(4, analysis.AverageGangSize, 3);
    }

    [Fact]
    public void Analyze_WhenResidentsNeverAttack_DescribesEveryPlayerAttacker()
    {
        // Only NPCs and visitors' own losses: every corporation is a victim, attackers have no corporation.
        var killmails = Enumerable.Range(0, 5)
            .Select(i => new KillmailBuilder(DAY.AddHours(i)).Victim(RESIDENT_CORP).Npc().Attacker(NPC_CORP, 0, characterId: null).Build())
            .ToList();

        var analysis = _sut.Analyze(killmails)!;

        Assert.False(analysis.FleetFromResidents);
        Assert.Empty(analysis.FleetHulls);
        Assert.Equal(0, analysis.AverageGangSize);
    }
}

using WHMapper.Models.DTO.ZKillboard;

namespace WHMapper.Tests.Services.WHIntel;

/// <summary>
/// Builds zKillboard killmails for intel tests.
/// </summary>
internal sealed class KillmailBuilder
{
    public const int SYSTEM_ID = 31000005;
    public const int STRUCTURE_CATEGORY = 65;
    public const int SHIP_CATEGORY = 6;

    private static long _nextKillmailId = 1;

    private readonly ZKillmail _killmail;

    public KillmailBuilder(DateTime killTimeUtc)
    {
        _killmail = new ZKillmail
        {
            KillmailId = _nextKillmailId++,
            KillmailTime = DateTime.SpecifyKind(killTimeUtc, DateTimeKind.Utc),
            SolarSystemId = SYSTEM_ID,
            Zkb = new ZKillmailZkb { Labels = [ZKillmailZkb.CATEGORY_LABEL_PREFIX + SHIP_CATEGORY] }
        };
    }

    public KillmailBuilder Victim(int corporationId, int? allianceId = null, int shipTypeId = 587, int? characterId = 90000001, int categoryId = SHIP_CATEGORY)
    {
        _killmail.Victim = new ZKillmailVictim
        {
            CharacterId = characterId,
            CorporationId = corporationId,
            AllianceId = allianceId,
            ShipTypeId = shipTypeId
        };
        _killmail.Zkb.Labels = [ZKillmailZkb.CATEGORY_LABEL_PREFIX + categoryId];
        return this;
    }

    public KillmailBuilder Attacker(int corporationId, int shipTypeId, int? allianceId = null, int? characterId = 90000002, bool finalBlow = false)
    {
        _killmail.Attackers.Add(new ZKillmailAttacker
        {
            CharacterId = characterId,
            CorporationId = corporationId,
            AllianceId = allianceId,
            ShipTypeId = shipTypeId,
            FinalBlow = finalBlow
        });
        return this;
    }

    public KillmailBuilder Npc()
    {
        _killmail.Zkb.Npc = true;
        return this;
    }

    public KillmailBuilder Solo()
    {
        _killmail.Zkb.Solo = true;
        return this;
    }

    public KillmailBuilder Value(double totalValue)
    {
        _killmail.Zkb.TotalValue = totalValue;
        return this;
    }

    public ZKillmail Build() => _killmail;
}

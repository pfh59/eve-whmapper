using System.Globalization;
using WHMapper.Services.ZKillboardAPI;

namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// Kill displayed in the intel dialog. Names are resolved through <see cref="WHIntelReport.GetName"/>.
/// </summary>
public sealed record WHIntelKill(
    long KillmailId,
    DateTime KillTimeUtc,
    int VictimShipTypeId,
    int? VictimCharacterId,
    int? VictimCorporationId,
    int? VictimAllianceId,
    int AttackerCount,
    int? FinalBlowCorporationId,
    double TotalValue,
    bool IsNpc,
    bool IsSolo)
{
    public string ZKillboardUrl => string.Format(CultureInfo.InvariantCulture, ZKillboardAPIServiceConstants.KILLMAIL_URL, KillmailId);
}

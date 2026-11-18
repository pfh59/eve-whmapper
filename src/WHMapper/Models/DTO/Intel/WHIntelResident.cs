namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// Corporation that probably lives in the system.
/// </summary>
/// <param name="StructureLosses">Structures the corporation lost in the system.</param>
/// <param name="ShipLosses">Other losses (ships, capsules, deployables) in the system.</param>
/// <param name="Kills">Killmails on which the corporation is an attacker.</param>
/// <param name="ActiveDays">Distinct UTC days on which the corporation appears on a killmail.</param>
/// <param name="Score">Residency score used for the ranking; higher is more likely.</param>
public sealed record WHIntelResident(
    int CorporationId,
    int? AllianceId,
    int StructureLosses,
    int ShipLosses,
    int Kills,
    int ActiveDays,
    int Score);

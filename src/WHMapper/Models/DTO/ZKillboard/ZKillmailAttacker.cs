using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.ZKillboard;

/// <summary>
/// Attacker of a zKillboard killmail. NPC attackers usually have no character id.
/// </summary>
public sealed class ZKillmailAttacker
{
    [JsonPropertyName("character_id")]
    public int? CharacterId { get; set; }

    [JsonPropertyName("corporation_id")]
    public int? CorporationId { get; set; }

    [JsonPropertyName("alliance_id")]
    public int? AllianceId { get; set; }

    [JsonPropertyName("ship_type_id")]
    public int? ShipTypeId { get; set; }

    [JsonPropertyName("final_blow")]
    public bool FinalBlow { get; set; }
}

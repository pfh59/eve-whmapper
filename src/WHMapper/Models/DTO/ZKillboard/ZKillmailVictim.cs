using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.ZKillboard;

/// <summary>
/// Victim of a zKillboard killmail.
/// </summary>
public sealed class ZKillmailVictim
{
    [JsonPropertyName("character_id")]
    public int? CharacterId { get; set; }

    [JsonPropertyName("corporation_id")]
    public int? CorporationId { get; set; }

    [JsonPropertyName("alliance_id")]
    public int? AllianceId { get; set; }

    [JsonPropertyName("ship_type_id")]
    public int ShipTypeId { get; set; }
}

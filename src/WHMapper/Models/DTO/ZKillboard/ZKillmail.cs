using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.ZKillboard;

/// <summary>
/// Killmail returned by the zKillboard API: the ESI killmail plus zKillboard metadata. Victim items are not mapped.
/// </summary>
public sealed class ZKillmail
{
    [JsonPropertyName("killmail_id")]
    public long KillmailId { get; set; }

    [JsonPropertyName("killmail_time")]
    public DateTime KillmailTime { get; set; }

    [JsonPropertyName("solar_system_id")]
    public int SolarSystemId { get; set; }

    [JsonPropertyName("victim")]
    public ZKillmailVictim Victim { get; set; } = new();

    [JsonPropertyName("attackers")]
    public List<ZKillmailAttacker> Attackers { get; set; } = new();

    [JsonPropertyName("zkb")]
    public ZKillmailZkb Zkb { get; set; } = new();
}

using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.ZKillboard;

/// <summary>
/// Entry of a zKillboard top list.
/// </summary>
public sealed class ZKillTopEntry
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("kills")]
    public int Kills { get; set; }
}

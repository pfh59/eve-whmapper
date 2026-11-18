using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.ZKillboard;

/// <summary>
/// Statistics of an entity returned by the zKillboard API. Only the top lists are mapped.
/// </summary>
public sealed class ZKillStats
{
    [JsonPropertyName("topLists")]
    public List<ZKillTopList> TopLists { get; set; } = new();
}

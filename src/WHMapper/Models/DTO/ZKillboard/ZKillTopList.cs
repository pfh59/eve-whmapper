using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.ZKillboard;

/// <summary>
/// One zKillboard top list, e.g. the top characters of a system.
/// </summary>
public sealed class ZKillTopList
{
    public const string TYPE_CHARACTER = "character";
    public const string TYPE_CORPORATION = "corporation";
    public const string TYPE_ALLIANCE = "alliance";

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public List<ZKillTopEntry> Values { get; set; } = new();
}

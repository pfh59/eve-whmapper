using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.ZKillboard;

/// <summary>
/// zKillboard metadata of a killmail.
/// </summary>
public sealed class ZKillmailZkb
{
    /// <summary>
    /// Label prefix carrying the inventory category of the victim, e.g. <c>cat:65</c> for a structure.
    /// </summary>
    public const string CATEGORY_LABEL_PREFIX = "cat:";

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("totalValue")]
    public double TotalValue { get; set; }

    [JsonPropertyName("npc")]
    public bool Npc { get; set; }

    [JsonPropertyName("solo")]
    public bool Solo { get; set; }

    [JsonPropertyName("awox")]
    public bool Awox { get; set; }

    [JsonPropertyName("labels")]
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Inventory category of the victim read from the labels; null when zKillboard did not label it.
    /// </summary>
    [JsonIgnore]
    public int? VictimCategoryId
    {
        get
        {
            string? label = Labels.FirstOrDefault(x => x.StartsWith(CATEGORY_LABEL_PREFIX, StringComparison.Ordinal));
            return label != null && int.TryParse(label.AsSpan(CATEGORY_LABEL_PREFIX.Length), out int categoryId)
                ? categoryId
                : null;
        }
    }
}

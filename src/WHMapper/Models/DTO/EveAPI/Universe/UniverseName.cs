using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe;

/// <summary>
/// Name of an EVE id resolved by ESI <c>POST /universe/names/</c>.
/// </summary>
public sealed class UniverseName
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}

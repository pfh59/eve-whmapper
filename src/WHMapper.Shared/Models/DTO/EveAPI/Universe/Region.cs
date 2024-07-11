using System.Text.Json.Serialization;

namespace WHMapper.Shared.Models.DTO.EveAPI.Universe
{
    public class Region
    {
        [JsonPropertyName("region_id")]
        public required int RegionId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("constellations")]
        public required int[] Constellations { get; set; }
    }
}


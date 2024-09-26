using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
    public class Constellation
	{
        [JsonPropertyName("constellation_id")]
        public required int ConstellationId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("position")]
        public required Position Position { get; set; }

        [JsonPropertyName("region_id")]
        public required int RegionId { get; set; }

        [JsonPropertyName("systems")]
        public required int[] Systems { get; set; }
    }
}


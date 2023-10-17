using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
    public class Star
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("solar_system_id")]
        public int SolarSystemId { get; set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        [JsonPropertyName("age")]
        public long Age { get; set; }

        [JsonPropertyName("luminosity")]
        public decimal Luminosity { get; set; }

        [JsonPropertyName("radius")]
        public long Radius { get; set; }

        [JsonPropertyName("spectral_class")]
        public string SpectralClass { get; set; }

        [JsonPropertyName("temperature")]
        public int Temperature { get; set; }
    }
}

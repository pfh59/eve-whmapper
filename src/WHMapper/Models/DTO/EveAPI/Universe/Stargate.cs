using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{

    public class Stargate
    {
        [JsonPropertyName("stargate_id")]
        public required int StargateId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("type_id")]
        public required int TypeId { get; set; }

        [JsonPropertyName("position")]
        public required Position Position { get; set; }
        
        [JsonPropertyName("system_id")]
        public required int SystemId { get; set; }

        [JsonPropertyName("destination")]
        public required Destination Destination { get; set; }
    }

    public class Destination
    {
        [JsonPropertyName("system_id")]
        public required int SystemId { get; set; }

        [JsonPropertyName("stargate_id")]
        public required int StargateId { get; set; }
    }
    
}


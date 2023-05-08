using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{

    public class Stargate
    {
        [JsonPropertyName("stargate_id")]
        public int StargateId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        [JsonPropertyName("position")]
        public Position Position { get; set; }
        
        [JsonPropertyName("system_id")]
        public int SystemId { get; set; }

        [JsonPropertyName("destination")]
        public Destination Destination { get; set; }
    }

    public class Destination
    {
        [JsonPropertyName("system_id")]
        public int SystemId { get; set; }

        [JsonPropertyName("stargate_id")]
        public int StargateId { get; set; }
    }
    
}


using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Route
{
    public class RouteRequest
    {
        [JsonPropertyName("avoid_systems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int[]? AvoidSystems { get; set; }

        [JsonPropertyName("connections")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RouteConnection[]? Connections { get; set; }

        [JsonPropertyName("preference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Preference { get; set; }

        [JsonPropertyName("security_penalty")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? SecurityPenalty { get; set; }
    }

    public class RouteConnection
    {
        [JsonPropertyName("from")]
        public int From { get; set; }

        [JsonPropertyName("to")]
        public int To { get; set; }
    }
}

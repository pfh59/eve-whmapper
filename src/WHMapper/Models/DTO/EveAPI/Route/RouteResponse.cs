using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Route
{
    public class RouteResponse
    {
        [JsonPropertyName("route")]
        public int[] Route { get; set; } = Array.Empty<int>();
    }
}

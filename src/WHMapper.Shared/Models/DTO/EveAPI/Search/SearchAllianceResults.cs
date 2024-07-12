using System.Text.Json.Serialization;

namespace WHMapper.Shared.Models.DTO.EveAPI.Search
{
    public class SearchAllianceResults
    {
        [JsonPropertyName("alliance")]
        public int[]? Alliances { get; set; } = null!;
    }
}


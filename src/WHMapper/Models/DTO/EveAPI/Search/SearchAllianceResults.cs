using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Search
{
    public class SearchAllianceResults
	{
        [JsonPropertyName("alliance")]
        public int[]? Alliances { get; set; } = null!;
    }
}


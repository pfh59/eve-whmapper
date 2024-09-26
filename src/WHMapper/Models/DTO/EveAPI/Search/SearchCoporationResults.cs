using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Search
{
    public class SearchCoporationResults
	{
        [JsonPropertyName("corporation")]
        public int[]? Corporations { get; set; }
    }
}


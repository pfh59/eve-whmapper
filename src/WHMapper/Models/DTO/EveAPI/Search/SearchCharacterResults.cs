using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Search
{
    public class SearchCharacterResults
	{
        [JsonPropertyName("character")]
        public int[]? Characters { get; set; }
    }
}


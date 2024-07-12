using System.Text.Json.Serialization;

namespace WHMapper.Shared.Models.DTO.EveAPI.Search
{
    public class SearchCharacterResults
    {
        [JsonPropertyName("character")]
        public int[]? Characters { get; set; }
    }
}


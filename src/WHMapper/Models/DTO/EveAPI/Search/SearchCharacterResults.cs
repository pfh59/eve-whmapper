using System;
using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Models.DTO.EveAPI.Search
{
	public class SearchCharacterResults
	{
        [JsonPropertyName("character")]
        public int[]? Characters { get; set; }
    }
}


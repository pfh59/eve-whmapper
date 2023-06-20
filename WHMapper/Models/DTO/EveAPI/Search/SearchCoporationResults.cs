using System;
using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Models.DTO.EveAPI.Search
{
	public class SearchAllainceResults
	{
        [JsonPropertyName("alliance")]
        public int[]? Alliances { get; set; }
    }
}


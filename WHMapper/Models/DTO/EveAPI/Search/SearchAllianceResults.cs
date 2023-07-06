using System;
using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Models.DTO.EveAPI.Search
{
	public class SearchAllianceResults
	{
        [JsonPropertyName("alliance")]
        public int[]? Alliances { get; set; } = null!;
    }
}


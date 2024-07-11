using System;
using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;

namespace WHMapper.Models.DTO.EveAPI.Search
{
	public class SearchCoporationResults
	{
        [JsonPropertyName("corporation")]
        public int[]? Corporations { get; set; }
    }
}


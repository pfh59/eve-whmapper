using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Character
{
	public class Character
	{
        [JsonPropertyName("alliance_id")]
        public int AllianceId { get; set; }

        [JsonPropertyName("birthday")]
        public DateTime Birthday { get; set; }

        [JsonPropertyName("bloodline_id")]
        public int BloodlineId { get; set; }

        [JsonPropertyName("corporation_id")]
        public int CorporationId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("faction_id")]
        public int FactionId { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("race_id")]
        public int RaceId { get; set; }

        [JsonPropertyName("security_status")]
        public float SecurityStatus { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
	}
}


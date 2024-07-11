using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Corporation
{
	public class Corporation
	{
        [JsonPropertyName("alliance_id")]
        public int AllianceId { get; set; }

        [JsonPropertyName("ceo_id")]
        public int CeoId { get; set; }

        [JsonPropertyName("creator_id")]
        public int CreatorId { get; set; }

        [JsonPropertyName("date_founded")]
        public DateTime DateFounded { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("faction_id")]
        public int FactionId { get; set; }

        [JsonPropertyName("home_station_id")]
        public int HomeStationId { get; set; }

        [JsonPropertyName("member_count")]
        public int MemberCount { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("shares")]
        public long Shares { get; set; }

        [JsonPropertyName("tax_rate")]
        public decimal TaxRate { get; set; }

        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("war_eligible")]
        public bool WarEligible { get; set; }
    }
}


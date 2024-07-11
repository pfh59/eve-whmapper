using System.Text.Json.Serialization;

namespace WHMapper.Shared.Models.DTO.EveAPI.Alliance
{
    public class Alliance
    {
        [JsonPropertyName("creator_corporation_id")]
        public int CreatorCorporationId { get; set; }

        [JsonPropertyName("creator_id")]
        public int CreatorId { get; set; }

        [JsonPropertyName("date_founded")]
        public DateTime DateFounded { get; set; }

        [JsonPropertyName("executor_corporation_id")]
        public int ExecutorCorporationId { get; set; }

        [JsonPropertyName("faction_id")]
        public int FactionId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = string.Empty;

        /*
        [JsonConstructor]
        public Alliance(int creatorCorporationIid, int creatorId, DateTime dateFounded,string name,string ticker) =>
            (CreatorCorporationId, CreatorId, DateFounded, Name, Ticker) = (creatorCorporationIid, creatorId, dateFounded,name, ticker);*/

    }
}


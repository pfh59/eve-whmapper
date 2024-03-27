using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Location
{
    public class Ship
    {
        [JsonPropertyName("ship_item_id")]
        public long ShipItemId { get; set; }

        [JsonPropertyName("ship_name")]
        public string? ShipName { get; set; }
        [JsonPropertyName("ship_type_id")]
        public int ShipTypeId { get; set; }
    }
}

using System;
using System.Text.Json.Serialization;
using static MudBlazor.FilterOperator;

namespace WHMapper.Models.DTO.EveAPI.Dogma
{
	public class Attribute
	{
        [JsonPropertyName("attribute_id")]
        public int AttributeId { get; set; }

        [JsonPropertyName("default_value")]
        public float DefaultValue { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("high_is_good")]
        public bool HighIsGood { get; set; }

        [JsonPropertyName("icon_id")]
        public int IconId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("published")]
        public bool Published { get; set; }

        [JsonPropertyName("stackable")]
        public bool Stackable { get; set; }

        [JsonPropertyName("unit_id")]
        public int UnitId { get; set; }


        public Attribute()
		{

		}
	}
}


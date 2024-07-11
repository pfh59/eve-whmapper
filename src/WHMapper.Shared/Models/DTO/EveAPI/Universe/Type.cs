using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
    public class Type
    {
        [JsonPropertyName("capacity")]
        public float Capacity { get; set; }

        [JsonInclude]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("dogma_attributes")]
        public Attribute[] DogmaAttributes { get; set; } =null!;

        [JsonPropertyName("dogma_effects")]
        public Effect[] DogmaEffects { get; set; } = null!;

        [JsonPropertyName("graphic_id")]
        public int GraphicId { get; set; }

        [JsonInclude]
        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        [JsonPropertyName("icon_id")]
        public int IconId { get; set; }

        [JsonPropertyName("market_group_id")]
        public int MarketGroupId { get; set; }

        [JsonPropertyName("mass")]
        public float Mass { get; set; }

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("packaged_volume")]
        public float PackagedVolume { get; set; }

        [JsonPropertyName("portion_size")]
        public int PortionSize { get; set; }

        [JsonInclude]
        [JsonPropertyName("published")]
        public bool Published { get; set; }

        [JsonPropertyName("radius")]
        public float Radius { get; set; }

        [JsonInclude]
        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        [JsonPropertyName("volume")]
        public float Volume { get; set; }
    }

    public class Attribute
    {
        [JsonInclude]
        [JsonPropertyName("attribute_id")]
        public int AttributeId { get; set; }

        [JsonInclude]
        [JsonPropertyName("value")]
        public float Value { get; set; }
    }

    public class Effect
    {
        [JsonInclude]
        [JsonPropertyName("effect_id")]
        public int EffectId { get; set; }

        [JsonInclude]
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
    }
}


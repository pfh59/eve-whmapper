using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
    public class Type
    {
        [JsonPropertyName("capacity")]
        public float Capacity { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        //[JsonPropertyName("dogma_attributes")]
        //public List<Attribute> DogmaAttributes { get; set; } = new List<Attribute>();

        //[JsonPropertyName("dogma_effects")]
        //public List<Effect> DogmaEffects { get; set; } = new List<Effect>();

        //[JsonPropertyName("graphic_id")]
        //public int GraphicId { get; set; }

        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        //[JsonPropertyName("icon_id")]
        //public int IconId { get; set; }

        //[JsonPropertyName("market_group_id")]
        //public int MarketGroupId { get; set; }

        [JsonPropertyName("mass")]
        public float Mass { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("packaged_volume")]
        public float PackagedVolume { get; set; }

        [JsonPropertyName("portion_size")]
        public int PortionSize { get; set; }

        [JsonPropertyName("published")]
        public bool Published { get; set; }

        [JsonPropertyName("radius")]
        public float Radius { get; set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; set; }

        [JsonPropertyName("volume")]
        public float Volume { get; set; }
    }

    public class Attribute
    {
        [JsonPropertyName("attribute_id")]
        public int AttributeId { get; set; }

        [JsonPropertyName("value")]
        public float Value { get; set; }
    }

    public class Effect
    {
        [JsonPropertyName("effect_id")]
        public int EffectId { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
    }
}


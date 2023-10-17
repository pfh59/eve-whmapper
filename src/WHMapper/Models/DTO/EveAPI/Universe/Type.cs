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
        public string Description { get; set; }

        [JsonPropertyName("dogma_attributes")]
        public Attribute[] DogmaAttributes { get; set; }

        [JsonPropertyName("dogma_effects")]
        public Effect[] DogmaEffects { get; set; }

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
        public string Name { get; set; }

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

        //[JsonConstructor]
        // public Type(string description, int group_id, string name,  bool published, int type_id )
        // => ( Description, GroupId, Name, Published,  TypeId) = ( description, group_id, name,  published, type_id);

        /*
        [JsonConstructor]
        public Type(float capacity, string description, Attribute[] dogma_attributes, Effect[] dogma_effects,int graphic_id, int group_id, float mass, string name, float packaged_volume, int portion_size,bool published, float radius,int type_id,float volume)
            => (Capacity, Description, DogmaAttributes, DogmaEffects,GraphicId, GroupId, Mass, Name, PackagedVolume, PortionSize, Published, Radius, TypeId, Volume) = (capacity, description, dogma_attributes, dogma_effects, graphic_id,group_id, mass, name, packaged_volume, portion_size, published, radius, type_id, volume);
        */
    }

    public class Attribute
    {
        [JsonPropertyName("attribute_id")]
        public int AttributeId { get; set; }

        [JsonPropertyName("value")]
        public float Value { get; set; }

        [JsonConstructor]
        public Attribute(int attributeId, float value)
            => (AttributeId, Value) = (attributeId, value);
    }

    public class Effect
    {
        [JsonPropertyName("effect_id")]
        public int EffectId { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        [JsonConstructor]
        public Effect(int effect_id, bool is_default)
            => (EffectId, IsDefault) = (effect_id, is_default);


    }
}


using System;
using System.Text.Json.Serialization;
using static MudBlazor.FilterOperator;

namespace WHMapper.Models.DTO.EveAPI.Dogma
{
	public class Effect
	{
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("disallow_auto_repeat")]
        public bool DisallowAutoRepeat { get; set; }

        [JsonPropertyName("discharge_attribute_id")]
        public int DischargeAttributeId { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("duration_attribute_id")]
        public int DurationAttributeId { get; set; }

        [JsonPropertyName("effect_category")]
        public int EffectCategory { get; set; }

        [JsonPropertyName("effect_id")]
        public int EffectId { get; set; }

        [JsonPropertyName("electronic_chance")]
        public bool ElectronicChance { get; set; }

        [JsonPropertyName("falloff_attribute_id")]
        public bool FalloffAttributeId { get; set; }

        [JsonPropertyName("icon_id")]
        public int IconId { get; set; }

        [JsonPropertyName("is_assistance")]
        public bool IsAssistance { get; set; }

        [JsonPropertyName("is_offensive")]
        public bool IsOffensive { get; set; }

        [JsonPropertyName("is_warp_safe")]
        public bool IsWarpSafe { get; set; }

        [JsonPropertyName("modifiers")]
        public Modifier[] Modifiers { get; set; } =null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("post_expression")]
        public int PostExpression { get; set; }

        [JsonPropertyName("pre_expression")]
        public int PreExpression { get; set; }

        [JsonPropertyName("published")]
        public bool published { get; set; }

        [JsonPropertyName("range_attribute_id")]
        public int RangeAttributeId { get; set; }

        [JsonPropertyName("range_chance")]
        public bool RangeChance { get; set; }

        [JsonPropertyName("tracking_speed_attribute_id")]
        public int TrackingSpeedAttributeId { get; set; }

        public Effect()
		{
		}
	}

    public class Modifier
    {
        [JsonPropertyName("func")]
        public string Func { get; set; } = string.Empty;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("modified_attribute_id")]
        public int ModifiedAttributeId { get; set; }

        [JsonPropertyName("modifying_attribute_id")]
        public int ModifyingAttributeId { get; set; }

        [JsonPropertyName("effect_id")]
        public int EffectId { get; set; }

        [JsonPropertyName("operator")]
        public int Operator { get; set; }


        public Modifier()
        { }
    }
}


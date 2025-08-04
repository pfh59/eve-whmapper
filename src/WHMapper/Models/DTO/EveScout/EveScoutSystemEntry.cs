using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveScout;

public enum ShipSize 
{
    Unknown,
    Small,
    Medium,
    Large,
    XLarge,
    Capital
}

public enum SignatureType
{
    Unknown,
    Cole
}

public enum SystemClass
{
    Unknown,
    C1,
    C2,
    C3,
    C4,
    C5,
    C6,
    C10,
    C11,
    C12,
    C13,
    C14,
    C15,
    C16,
    C17,
    C18,
    C25,
    Drone,
    Exit,
    HS,
    Jove,
    LS,
    NS
}

public class EveScoutSystemEntry
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("created_by_id")]
    public int CreatedById { get; set; }

    [JsonPropertyName("created_by_name")]
    public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int? UpdatedById { get; set; }

    [JsonPropertyName("updated_by_name")]
    public string? UpdatedByName { get; set; }

    [JsonPropertyName("completed_at")]
    public string? CompletedAt { get; set; }

    [JsonPropertyName("completed_by_id")]
    public int? CompletedById { get; set; }

    [JsonPropertyName("completed_by_name")]
    public string? CompletedByName { get; set; }

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("wh_exits_outward")]
    public bool? WhExitsOutward { get; set; }

    [JsonPropertyName("wh_type")]
    public string? WhType { get; set; }

    [JsonPropertyName("max_ship_size")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ShipSize MaxShipSize { get; set; } = ShipSize.Unknown;

    [JsonPropertyName("expires_at")]
    public string ExpiresAt { get; set; } = string.Empty;

    [JsonPropertyName("remaining_hours")]
    public int? RemainingHours { get; set; }

    [JsonPropertyName("signature_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SignatureType SignatureType { get; set; } = SignatureType.Unknown;

    [JsonPropertyName("out_system_id")]
    public int OutSystemId { get; set; }

    [JsonPropertyName("out_system_name")]
    public string OutSystemName { get; set; } = string.Empty;

    [JsonPropertyName("out_signature")]
    public string OutSignature { get; set; } = string.Empty;

    [JsonPropertyName("in_system_id")]
    public int? InSystemId { get; set; }

    [JsonPropertyName("in_system_class")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SystemClass? InSystemClass { get; set; } = SystemClass.Unknown;

    [JsonPropertyName("in_system_name")]
    public string? InSystemName { get; set; }

    [JsonPropertyName("in_region_id")]
    public int? InRegionId { get; set; }

    [JsonPropertyName("in_region_name")]
    public string? InRegionName { get; set; }

    [JsonPropertyName("in_signature")]
    public string? InSignature { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }


}
using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveScout;

using System.Runtime.Serialization;

public enum ShipSize
{
    [EnumMember(Value = "small")]
    Small,
    [EnumMember(Value = "medium")]
    Medium,
    [EnumMember(Value = "large")]
    Large,
    [EnumMember(Value = "xlarge")]
    Xlarge,
    [EnumMember(Value = "capital")]
    Capital,
    [EnumMember(Value = "unknown")]
    Unknown
}

public enum SignatureType
{
    [EnumMember(Value = "combat")]
    Combat,
    [EnumMember(Value = "data")]
    Data,
    [EnumMember(Value = "gas")]
    Gas,
    [EnumMember(Value = "relic")]
    Relic,
    [EnumMember(Value = "wormhole")]
    Wormhole,
    [EnumMember(Value = "unknown")]
    Unknown
}


public enum SystemClass
{
    [EnumMember(Value = "c1")]
    C1,
    [EnumMember(Value = "c2")]
    C2,
    [EnumMember(Value = "c3")]
    C3,
    [EnumMember(Value = "c4")]
    C4,
    [EnumMember(Value = "c5")]
    C5,
    [EnumMember(Value = "c6")]
    C6,
    [EnumMember(Value = "c10")]
    C10,
    [EnumMember(Value = "c11")]
    C11,
    [EnumMember(Value = "c12")]
    C12,
    [EnumMember(Value = "c13")]
    C13,
    [EnumMember(Value = "c14")]
    C14,
    [EnumMember(Value = "c15")]
    C15,
    [EnumMember(Value = "c16")]
    C16,
    [EnumMember(Value = "c17")]
    C17,
    [EnumMember(Value = "c18")]
    C18,
    [EnumMember(Value = "c25")]
    C25,
    [EnumMember(Value = "drone")]
    Drone,
    [EnumMember(Value = "exit")]
    Exit,
    [EnumMember(Value = "hs")]
    Hs,
    [EnumMember(Value = "jove")]
    Jove,
    [EnumMember(Value = "ls")]
    Ls,
    [EnumMember(Value = "ns")]
    Ns,
    [EnumMember(Value = "unknown")]
    Unknown
}

public class EveScoutSystemEntry
{
    [JsonPropertyName("id")]    
    public int Id { get; set; } 

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("created_by_id")]
    public int CreatedById { get; set; }

    [JsonPropertyName("created_by_name")]
    public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int? UpdatedById { get; set; }

    [JsonPropertyName("updated_by_name")]
    public string? UpdatedByName { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

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
    public DateTime ExpiresAt { get; set; }

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

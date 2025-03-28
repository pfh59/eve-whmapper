using System;
using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Character;

public class Portrait
{

    [JsonPropertyName("px128x128")]
    public string Picture128x128 { get; set; }

    [JsonPropertyName("px256x256")]
    public string Picture256x256 { get; set; }

    [JsonPropertyName("px512x512")]
    public string Picture512x512 { get; set; }

    [JsonPropertyName("px64x64")]
    public string Picture64x64 { get; set; }

}

using System.Text.Json.Serialization;

namespace WHMapper;

public class AssetName
{
    [JsonPropertyName("item_id")]
    public long ItemId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonConstructor]
    public AssetName(long item_id, string name) =>
        (ItemId, Name) = (item_id, name);
}

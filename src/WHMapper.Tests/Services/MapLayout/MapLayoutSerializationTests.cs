using System.Text.Json;
using WHMapper.Models.DTO.MapLayout;
using WHMapper.Services.MapLayout;

namespace WHMapper.Tests.Services.MapLayout;

public class MapLayoutSerializationTests
{
    [Fact]
    public void MapLayoutDto_RoundTripsThroughSystemTextJson()
    {
        var original = new MapLayoutDto(
            MapLayoutService.CURRENT_LAYOUT_VERSION,
            [
                new MapPanelLayoutDto(nameof(MapPanelId.Signatures), 120.5, 240.25, true, false),
                new MapPanelLayoutDto(nameof(MapPanelId.Notes), 10, 20, false, true)
            ]);

        var restored = JsonSerializer.Deserialize<MapLayoutDto>(JsonSerializer.Serialize(original));

        Assert.NotNull(restored);
        Assert.Equal(original.Version, restored.Version);
        Assert.Equal(original.Panels, restored.Panels);
    }

    [Fact]
    public void MapLayoutDto_TruncatedPayload_ThrowsJsonExceptionCaughtByTheStorageLayer()
    {
        var truncated = JsonSerializer.Serialize(
            new MapLayoutDto(MapLayoutService.CURRENT_LAYOUT_VERSION, []))[..10];

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<MapLayoutDto>(truncated));
    }
}

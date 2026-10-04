using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Tests.Models.DTO.MapLayout;

public class MapPanelDefaultsTests
{
    private const MapPanelId UNKNOWN_PANEL_ID = (MapPanelId)999;

    [Fact]
    public void GetPosition_WhenPanelIsUnknown_ReturnsTopLeftCorner()
    {
        var (x, y) = MapPanelDefaults.GetPosition(UNKNOWN_PANEL_ID, 1600, 900);

        Assert.Equal(x, y);
        Assert.InRange(x, 0, MapPanelDefaults.MIN_VISIBLE_WIDTH);
    }

    [Fact]
    public void GetDefaultSize_OnlySizesTheSignaturesPanel()
    {
        Assert.All(
            Enum.GetValues<MapPanelId>().Where(id => id != MapPanelId.Signatures),
            id => Assert.Null(MapPanelDefaults.GetDefaultSize(id)));

        var size = MapPanelDefaults.GetDefaultSize(MapPanelId.Signatures);
        Assert.NotNull(size);
        Assert.Equal(MapPanelDefaults.GetWidth(MapPanelId.Signatures), size.Value.Width);
        Assert.True(size.Value.Height >= MapPanelDefaults.MIN_RESIZABLE_HEIGHT);
    }

    [Fact]
    public void GetWidth_WhenPanelIsUnknown_ReturnsAPositiveWidth()
    {
        Assert.True(MapPanelDefaults.GetWidth(UNKNOWN_PANEL_ID) > 0);
    }
}

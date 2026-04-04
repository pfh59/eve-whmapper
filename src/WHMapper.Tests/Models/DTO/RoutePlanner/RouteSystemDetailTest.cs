using WHMapper.Models.DTO.RoutePlanner;
using Xunit;

namespace WHMapper.Tests.Models.DTO.RoutePlanner;

public class RouteSystemDetailTest
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var systemId = 30000142;
        var systemName = "Jita";
        var color = "#FF0000";

        var detail = new RouteSystemDetail(systemId, systemName, color);

        Assert.Equal(systemId, detail.SystemId);
        Assert.Equal(systemName, detail.SystemName);
        Assert.Equal(color, detail.Color);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var detail = new RouteSystemDetail(1, "Initial", "blue");

        detail.SystemId = 30000144;
        detail.SystemName = "Amarr";
        detail.Color = "green";

        Assert.Equal(30000144, detail.SystemId);
        Assert.Equal("Amarr", detail.SystemName);
        Assert.Equal("green", detail.Color);
    }
}

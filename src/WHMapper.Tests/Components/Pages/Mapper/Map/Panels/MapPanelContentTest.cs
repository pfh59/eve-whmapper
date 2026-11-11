using WHMapper.Components.Pages.Mapper.Map.Panels;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.MapLayout;
using Xunit;

namespace WHMapper.Tests.Components.Pages.Mapper.Map.Panels;

public class MapPanelContentTest
{
    /// <summary>Exposes the render guard under test.</summary>
    private sealed class TestableMapPanelContent : MapPanelContent
    {
        public bool CallShouldRender() => ShouldRender();
    }

    private static EveSystemLinkModel CreateLinkModel()
    {
        var whSystem = new WHSystem(1, 1, "Test System", 0.5f, 0.0, 0.0);
        var node = new EveSystemNodeModel(whSystem, null, string.Empty, string.Empty);
        var whLink = new WHSystemLink(1, 2, 3) { Id = 1 };
        return new EveSystemLinkModel(whLink, node, node);
    }

    private static TestableMapPanelContent CreateRenderedLinkPanel(EveSystemLinkModel link)
    {
        var content = new TestableMapPanelContent { PanelId = MapPanelId.LinkInfos, SelectedSystemLink = link };
        Assert.True(content.CallShouldRender());
        return content;
    }

    [Fact]
    public void ShouldRender_WhenInputsUnchanged_ReturnsFalse()
    {
        var content = CreateRenderedLinkPanel(CreateLinkModel());

        Assert.False(content.CallShouldRender());
    }

    [Fact]
    public void ShouldRender_WhenLinkEolStatusChangedInPlace_ReturnsTrue()
    {
        var link = CreateLinkModel();
        var content = CreateRenderedLinkPanel(link);

        link.EndOfLifeStatus = SystemLinkEolStatus.EOL1h;

        Assert.True(content.CallShouldRender());
    }

    [Fact]
    public void ShouldRender_WhenLinkMassStatusChangedInPlace_ReturnsTrue()
    {
        var link = CreateLinkModel();
        var content = CreateRenderedLinkPanel(link);

        link.MassStatus = SystemLinkMassStatus.Verge;

        Assert.True(content.CallShouldRender());
    }

    [Fact]
    public void ShouldRender_WhenLinkSizeChangedInPlace_ReturnsTrue()
    {
        var link = CreateLinkModel();
        link.Size = SystemLinkSize.Large;
        var content = CreateRenderedLinkPanel(link);

        link.Size = SystemLinkSize.Small;

        Assert.True(content.CallShouldRender());
    }
}

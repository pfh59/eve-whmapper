using Blazor.Diagrams.Core.Layers;
using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Components.Pages.Mapper.Map.Panels;

/// <summary>
/// Renders the body of one map panel.
/// </summary>
/// <remarks>Skips renders with unchanged inputs: a drag re-renders the overlay on every pointer move.</remarks>
public partial class MapPanelContent
{
    /// <summary>Panel whose body must be rendered.</summary>
    [Parameter, EditorRequired]
    public MapPanelId PanelId { get; set; }

    /// <summary>Map the panels belong to.</summary>
    [Parameter]
    public int? MapId { get; set; }

    /// <summary>Currently selected system, or <c>null</c> when none is selected.</summary>
    [Parameter]
    public EveSystemNodeModel? SelectedSystemNode { get; set; }

    /// <summary>Currently selected connection, or <c>null</c> when none is selected.</summary>
    [Parameter]
    public EveSystemLinkModel? SelectedSystemLink { get; set; }

    /// <summary>Primary account id, resolved once by the map to keep the render path non-blocking.</summary>
    [Parameter]
    public int? PrimaryUserId { get; set; }

    /// <summary>Diagram links, used by the route planner to route through mapped wormholes.</summary>
    [Parameter]
    public LinkLayer? Links { get; set; }

    private PanelInputs? _renderedInputs;

    /// <inheritdoc />
    protected override bool ShouldRender()
    {
        var inputs = new PanelInputs(PanelId, MapId, SelectedSystemNode, SelectedSystemLink, PrimaryUserId, Links);
        if (inputs == _renderedInputs)
        {
            return false;
        }

        _renderedInputs = inputs;
        return true;
    }

    /// <summary>Inputs of the body; references compare by identity.</summary>
    private sealed record PanelInputs(
        MapPanelId PanelId,
        int? MapId,
        EveSystemNodeModel? Node,
        EveSystemLinkModel? Link,
        int? PrimaryUserId,
        LinkLayer? Links);
}

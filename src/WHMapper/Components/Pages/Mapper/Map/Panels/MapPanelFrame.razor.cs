using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Components.Pages.Mapper.Map.Panels;

/// <summary>
/// Chrome of a map panel: title bar used to move it, collapse and close buttons.
/// </summary>
/// <remarks>Only reports pointer offsets; the overlay owns and clamps the position.</remarks>
public partial class MapPanelFrame
{
    /// <summary>Panel being rendered.</summary>
    [Parameter, EditorRequired]
    public MapPanelId PanelId { get; set; }

    /// <summary>Distance in pixels from the left edge of the map.</summary>
    [Parameter]
    public double X { get; set; }

    /// <summary>Distance in pixels from the top edge of the map.</summary>
    [Parameter]
    public double Y { get; set; }

    /// <summary>Stacking order of the panel.</summary>
    [Parameter]
    public int ZOrder { get; set; }

    /// <summary>Whether only the title bar is shown.</summary>
    [Parameter]
    public bool IsCollapsed { get; set; }

    /// <summary>Body of the panel.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Raised when the user closes the panel.</summary>
    [Parameter]
    public EventCallback<MapPanelId> OnClose { get; set; }

    /// <summary>Raised when the user collapses or expands the panel.</summary>
    [Parameter]
    public EventCallback<MapPanelId> OnToggleCollapse { get; set; }

    /// <summary>Raised when the panel is pressed or starts moving, to bring it above the others.</summary>
    [Parameter]
    public EventCallback<MapPanelId> OnActivated { get; set; }

    /// <summary>Raised for each pointer move while the title bar is dragged, with the offset in pixels.</summary>
    [Parameter]
    public EventCallback<(MapPanelId PanelId, double DeltaX, double DeltaY)> OnMove { get; set; }

    /// <summary>Raised once when a move ends.</summary>
    [Parameter]
    public EventCallback<MapPanelId> OnMoveEnded { get; set; }

    private MudSwipeArea? _swipeArea;
    private bool _isMoving;

    private string Title => MapPanelPresentation.GetTitle(PanelId);

    private string Icon => MapPanelPresentation.GetIcon(PanelId);

    private string Width => string.Create(CultureInfo.InvariantCulture, $"{MapPanelDefaults.GetWidth(PanelId)}px");

    private string PositionStyle => string.Create(
        CultureInfo.InvariantCulture, $"position:absolute;left:{X}px;top:{Y}px;z-index:{ZOrder};");

    private string DragAreaClass => _isMoving ? "whm-panel__drag whm-panel__drag--moving" : "whm-panel__drag";

    private async Task OnSwipeMoveAsync(MultiDimensionSwipeEventArgs args)
    {
        if (!_isMoving)
        {
            _isMoving = true;
            await OnActivated.InvokeAsync(PanelId);
        }

        // Deltas are "previous minus current" pointer position, hence the sign flip.
        await OnMove.InvokeAsync((PanelId, -(args.SwipeDeltas[0] ?? 0), -(args.SwipeDeltas[1] ?? 0)));
    }

    private async Task EndMoveAsync()
    {
        // A pointer released outside the area never reaches it, so the swipe is reset explicitly.
        _swipeArea?.Cancel();

        if (!_isMoving)
        {
            return;
        }

        _isMoving = false;
        await OnMoveEnded.InvokeAsync(PanelId);
    }
}

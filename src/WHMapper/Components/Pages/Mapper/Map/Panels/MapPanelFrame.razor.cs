using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Components.Pages.Mapper.Map.Panels;

/// <summary>
/// Chrome of a map panel: title bar used to move it, collapse and close buttons, resize grip.
/// </summary>
/// <remarks>Only reports pointer offsets; the overlay owns and clamps the position and size.</remarks>
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

    /// <summary>Width of the panel, in pixels.</summary>
    [Parameter]
    public double Width { get; set; }

    /// <summary>Whether a grip lets the user resize the panel.</summary>
    [Parameter]
    public bool IsResizable { get; set; }

    /// <summary>Stacking order of the panel.</summary>
    [Parameter]
    public int ZOrder { get; set; }

    /// <summary>Complement of the title, e.g. the displayed system name.</summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>Whether a mask blocks interaction with the body, e.g. when it shows a stale selection.</summary>
    [Parameter]
    public bool IsMasked { get; set; }

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

    /// <summary>Raised for each pointer move while the grip is dragged, with the size offset in pixels.</summary>
    [Parameter]
    public EventCallback<(MapPanelId PanelId, double DeltaWidth, double DeltaHeight)> OnResize { get; set; }

    /// <summary>Raised once when a resize ends.</summary>
    [Parameter]
    public EventCallback<MapPanelId> OnResizeEnded { get; set; }

    private MudSwipeArea? _swipeArea;
    private MudSwipeArea? _resizeArea;
    private bool _isMoving;
    private bool _isResizing;

    private string Title => string.IsNullOrEmpty(Subtitle)
        ? MapPanelPresentation.GetTitle(PanelId)
        : $"{MapPanelPresentation.GetTitle(PanelId)} - {Subtitle}";

    private string Icon => MapPanelPresentation.GetIcon(PanelId);

    private string WidthStyle => MapPanelPresentation.ToCssPixels(Width);

    private string PositionStyle => string.Create(
        CultureInfo.InvariantCulture, $"position:absolute;left:{X}px;top:{Y}px;z-index:{ZOrder};");

    private string DragAreaClass => _isMoving ? "whm-panel__drag whm-panel__drag--moving" : "whm-panel__drag";

    private string ResizeGripClass => _isResizing ? "whm-panel__resize whm-panel__resize--active" : "whm-panel__resize";

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

    private async Task OnResizeSwipeMoveAsync(MultiDimensionSwipeEventArgs args)
    {
        if (!_isResizing)
        {
            _isResizing = true;
            await OnActivated.InvokeAsync(PanelId);
        }

        await OnResize.InvokeAsync((PanelId, -(args.SwipeDeltas[0] ?? 0), -(args.SwipeDeltas[1] ?? 0)));
    }

    private async Task EndResizeAsync()
    {
        _resizeArea?.Cancel();

        if (!_isResizing)
        {
            return;
        }

        _isResizing = false;
        await OnResizeEnded.InvokeAsync(PanelId);
    }
}

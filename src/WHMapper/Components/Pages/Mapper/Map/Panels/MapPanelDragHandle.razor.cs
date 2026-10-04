using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace WHMapper.Components.Pages.Mapper.Map.Panels;

/// <summary>
/// Area of a map panel that reports pointer drags, e.g. the title bar or the resize grip.
/// </summary>
/// <remarks>
/// MudSwipeArea has no pointer capture: while dragging, <see cref="ActiveClass"/> must render a viewport-wide
/// layer of the area, and leaving the area with a button still pressed does not end the drag.
/// </remarks>
public partial class MapPanelDragHandle
{
    /// <summary>Primary button value of <see cref="MouseEventArgs.Button"/>.</summary>
    private const long PRIMARY_BUTTON = 0;

    /// <summary>Value of <see cref="MouseEventArgs.Buttons"/> when no button is pressed.</summary>
    private const long NO_BUTTONS = 0;

    /// <summary>CSS class of the area.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>CSS class added while dragging.</summary>
    [Parameter]
    public string? ActiveClass { get; set; }

    /// <summary>Content of the area.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Raised when the area is pressed with the primary button.</summary>
    [Parameter]
    public EventCallback OnDragStarted { get; set; }

    /// <summary>Raised for each pointer move while dragging, with the offset in pixels.</summary>
    [Parameter]
    public EventCallback<(double DeltaX, double DeltaY)> OnDrag { get; set; }

    /// <summary>Raised once when a drag ends.</summary>
    [Parameter]
    public EventCallback OnDragEnded { get; set; }

    private MudSwipeArea? _swipeArea;
    private bool _isDragging;

    private string AreaClass => _isDragging ? $"{Class} {ActiveClass}" : Class ?? string.Empty;

    private async Task BeginDragAsync(PointerEventArgs args)
    {
        if (args.Button != PRIMARY_BUTTON)
        {
            return;
        }

        _isDragging = true;
        await OnDragStarted.InvokeAsync();
    }

    private async Task OnSwipeMoveAsync(MultiDimensionSwipeEventArgs args)
    {
        if (!_isDragging)
        {
            return;
        }

        // The button was released where the area could not see it, e.g. outside the window.
        if (args.TouchEventArgs.Buttons == NO_BUTTONS)
        {
            await EndDragAsync();
            return;
        }

        // Deltas are "previous minus current" pointer position, hence the sign flip.
        await OnDrag.InvokeAsync((-(args.SwipeDeltas[0] ?? 0), -(args.SwipeDeltas[1] ?? 0)));
    }

    private Task OnSwipeLeaveAsync(PointerEventArgs args) =>
        // A pointer that outruns the area before the viewport-wide layer renders is still dragging.
        args.Buttons == NO_BUTTONS ? EndDragAsync() : Task.CompletedTask;

    private async Task EndDragAsync()
    {
        // A pointer released outside the area never reaches it, so the swipe is reset explicitly.
        _swipeArea?.Cancel();

        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        await OnDragEnded.InvokeAsync();
    }
}

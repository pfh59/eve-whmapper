using Blazor.Diagrams.Core.Layers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.MapLayout;
using WHMapper.Services.MapLayout;

namespace WHMapper.Components.Pages.Mapper.Map.Panels;

/// <summary>
/// Hosts the freely positioned map panels and persists their layout.
/// </summary>
/// <remarks>A component of its own so that the per-move renders of a drag never re-render the diagram canvas.</remarks>
public partial class MapPanelOverlay : IAsyncDisposable
{
    /// <summary>Height of the application bar and map tabs, excluded from the map area.</summary>
    private const double CHROME_HEIGHT_ALLOWANCE = 112;

    [Inject]
    private IMapLayoutService MapLayoutService { get; set; } = null!;

    [Inject]
    private IBrowserViewportService BrowserViewportService { get; set; } = null!;

    /// <summary>Map the panels belong to; also the persistence scope.</summary>
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

    /// <summary>Diagram links, used by the route planner.</summary>
    [Parameter]
    public LinkLayer? Links { get; set; }

    private readonly Guid _viewportObserverId = Guid.NewGuid();
    private IReadOnlyList<MapPanelLayout> _panels = [];
    private int? _layoutLoadedForMapId;
    private double _containerWidth = MapPanelDefaults.FALLBACK_CONTAINER_WIDTH;
    private double _containerHeight = MapPanelDefaults.FALLBACK_CONTAINER_HEIGHT;

    private IEnumerable<MapPanelLayout> VisiblePanels =>
        _panels.Where(panel => panel.IsUserVisible && IsPanelApplicable(panel.Id));

    /// <summary>
    /// Tells whether a panel is relevant to the current selection, independently of its user visibility.
    /// </summary>
    private bool IsPanelApplicable(MapPanelId panelId) => panelId switch
    {
        MapPanelId.LinkInfos => SelectedSystemLink is not null,
        _ => SelectedSystemNode is not null
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            // Existing panels are not reflowed on resize; only the clamp bounds follow the window.
            await BrowserViewportService.SubscribeAsync(_viewportObserverId,
                args => UpdateContainerSize(args.BrowserWindowSize), fireImmediately: false);
        }

        // Loaded after render: the browser size and ProtectedLocalStorage need a live circuit.
        if (_layoutLoadedForMapId != MapId)
        {
            _layoutLoadedForMapId = MapId;
            UpdateContainerSize(await BrowserViewportService.GetCurrentBrowserWindowSizeAsync());
            _panels = await MapLayoutService.LoadAsync(MapId ?? 0, _containerWidth, _containerHeight);
            StateHasChanged();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await BrowserViewportService.UnsubscribeAsync(_viewportObserverId);
        GC.SuppressFinalize(this);
    }

    private void UpdateContainerSize(BrowserWindowSize? windowSize)
    {
        if (windowSize is null || windowSize.Width <= 0 || windowSize.Height <= 0)
        {
            return;
        }

        _containerWidth = windowSize.Width;
        _containerHeight = Math.Max(MapPanelDefaults.TITLE_BAR_HEIGHT, windowSize.Height - CHROME_HEIGHT_ALLOWANCE);
    }

    private void MovePanel((MapPanelId PanelId, double DeltaX, double DeltaY) move) =>
        _panels = MapLayoutService.MoveBy(
            _panels, move.PanelId, move.DeltaX, move.DeltaY, _containerWidth, _containerHeight);

    private void BringPanelToFront(MapPanelId panelId) =>
        _panels = MapLayoutService.BringToFront(_panels, panelId);

    private Task ClosePanelAsync(MapPanelId panelId) =>
        ApplyAndPersistAsync(MapLayoutService.Close(_panels, panelId));

    private Task TogglePanelVisibilityAsync(MapPanelId panelId) =>
        ApplyAndPersistAsync(MapLayoutService.ToggleVisibility(_panels, panelId));

    private Task ToggleCollapseAsync(MapPanelId panelId) =>
        ApplyAndPersistAsync(MapLayoutService.ToggleCollapse(_panels, panelId));

    private async Task ResetLayoutAsync() =>
        _panels = await MapLayoutService.ResetAsync(MapId ?? 0, _containerWidth, _containerHeight);

    private Task ApplyAndPersistAsync(IReadOnlyList<MapPanelLayout> panels)
    {
        _panels = panels;
        return PersistLayoutAsync();
    }

    private Task PersistLayoutAsync() => MapLayoutService.ScheduleSaveAsync(MapId ?? 0, _panels);
}

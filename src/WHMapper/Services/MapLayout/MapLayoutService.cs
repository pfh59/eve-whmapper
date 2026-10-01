using System.Collections.Concurrent;
using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Services.MapLayout;

/// <summary>
/// Default <see cref="IMapLayoutService"/>, debouncing writes to <see cref="IMapLayoutStorage"/>.
/// </summary>
public sealed class MapLayoutService : IMapLayoutService, IAsyncDisposable
{
    /// <summary>Schema version written to storage; a stored payload of any other version is discarded.</summary>
    public const int CURRENT_LAYOUT_VERSION = 4;

    /// <summary>Quiet period that coalesces rapid layout changes into one storage write.</summary>
    public const int LAYOUT_SAVE_DEBOUNCE_MS = 500;

    /// <summary>Highest stacking order a panel may reach, below the MudBlazor popover z-index (1100).</summary>
    public const int MAX_PANEL_Z_ORDER = 999;

    private const int MIN_PANEL_Z_ORDER = 10;

    private readonly IMapLayoutStorage _storage;
    private readonly ILogger<MapLayoutService> _logger;
    private readonly ConcurrentDictionary<int, PendingSave> _pendingSaves = new();

    public MapLayoutService(IMapLayoutStorage storage, ILogger<MapLayoutService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <inheritdoc />
    public IReadOnlyList<MapPanelLayout> CreateDefaultLayout(double containerWidth, double containerHeight) =>
        Enum.GetValues<MapPanelId>()
            .Select((panelId, index) =>
            {
                var panel = new MapPanelLayout { Id = panelId, ZOrder = MIN_PANEL_Z_ORDER + index };
                var (x, y) = MapPanelDefaults.GetPosition(panelId, containerWidth, containerHeight);
                ApplyClampedPosition(panel, x, y, containerWidth, containerHeight);
                return panel;
            })
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<MapPanelLayout>> LoadAsync(int mapId, double containerWidth, double containerHeight) =>
        Merge(await _storage.GetAsync(mapId), containerWidth, containerHeight);

    /// <inheritdoc />
    public IReadOnlyList<MapPanelLayout> Merge(MapLayoutDto? stored, double containerWidth, double containerHeight)
    {
        var panels = CreateDefaultLayout(containerWidth, containerHeight);

        if (stored is null || stored.Version != CURRENT_LAYOUT_VERSION || stored.Panels is null)
        {
            return panels;
        }

        foreach (var storedPanel in stored.Panels)
        {
            if (!Enum.TryParse<MapPanelId>(storedPanel.PanelId, ignoreCase: true, out var panelId)
                || panels.FirstOrDefault(p => p.Id == panelId) is not { } panel)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Ignoring unknown stored panel {PanelId}", storedPanel.PanelId);
                }

                continue;
            }

            if (double.IsFinite(storedPanel.X) && double.IsFinite(storedPanel.Y))
            {
                ApplyClampedPosition(panel, storedPanel.X, storedPanel.Y, containerWidth, containerHeight);
            }

            panel.IsUserVisible = storedPanel.Visible;
            panel.IsCollapsed = storedPanel.Collapsed;
        }

        return panels;
    }

    /// <inheritdoc />
    public IReadOnlyList<MapPanelLayout> MoveBy(
        IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId,
        double deltaX, double deltaY, double containerWidth, double containerHeight)
    {
        var panel = panels.FirstOrDefault(p => p.Id == panelId);
        if (panel is null || !double.IsFinite(deltaX) || !double.IsFinite(deltaY))
        {
            return panels;
        }

        ApplyClampedPosition(panel, panel.X + deltaX, panel.Y + deltaY, containerWidth, containerHeight);
        return panels;
    }

    /// <inheritdoc />
    public IReadOnlyList<MapPanelLayout> BringToFront(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId)
    {
        var panel = panels.FirstOrDefault(p => p.Id == panelId);
        if (panel is null)
        {
            return panels;
        }

        var highest = panels.Max(p => p.ZOrder);
        if (panel.ZOrder == highest && panels.Count(p => p.ZOrder == highest) == 1)
        {
            return panels;
        }

        if (highest >= MAX_PANEL_Z_ORDER)
        {
            // Compact instead of overflowing into the popover band.
            var ordered = panels.OrderBy(p => p.ZOrder).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].ZOrder = MIN_PANEL_Z_ORDER + i;
            }

            highest = panels.Max(p => p.ZOrder);
        }

        panel.ZOrder = highest + 1;
        return panels;
    }

    /// <inheritdoc />
    public IReadOnlyList<MapPanelLayout> Close(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId) =>
        Mutate(panels, panelId, panel => panel.IsUserVisible = false);

    /// <inheritdoc />
    public IReadOnlyList<MapPanelLayout> ToggleVisibility(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId) =>
        Mutate(panels, panelId, panel => panel.IsUserVisible = !panel.IsUserVisible);

    /// <inheritdoc />
    public IReadOnlyList<MapPanelLayout> ToggleCollapse(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId) =>
        Mutate(panels, panelId, panel => panel.IsCollapsed = !panel.IsCollapsed);

    /// <inheritdoc />
    public Task ScheduleSaveAsync(int mapId, IReadOnlyList<MapPanelLayout> panels)
    {
        var snapshot = ToDto(panels);

        _pendingSaves.AddOrUpdate(
            mapId,
            key => StartSave(key, snapshot),
            (key, existing) =>
            {
                existing.Cancellation.Cancel();
                existing.Cancellation.Dispose();
                return StartSave(key, snapshot);
            });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task FlushAsync(int mapId)
    {
        if (!_pendingSaves.TryGetValue(mapId, out var pending))
        {
            return;
        }

        await pending.Task;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MapPanelLayout>> ResetAsync(int mapId, double containerWidth, double containerHeight)
    {
        if (_pendingSaves.TryRemove(mapId, out var pending))
        {
            await pending.Cancellation.CancelAsync();
            pending.Cancellation.Dispose();
        }

        await _storage.RemoveAsync(mapId);
        return CreateDefaultLayout(containerWidth, containerHeight);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        // No final flush: the circuit is usually gone by now and ProtectedLocalStorage would throw.
        foreach (var cancellation in _pendingSaves.Values.Select(pending => pending.Cancellation))
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }

        _pendingSaves.Clear();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Keeps at least <see cref="MapPanelDefaults.MIN_VISIBLE_WIDTH"/> of the panel and its whole title bar inside the map.
    /// </summary>
    private static void ApplyClampedPosition(
        MapPanelLayout panel, double x, double y, double containerWidth, double containerHeight)
    {
        var width = containerWidth > 0 ? containerWidth : MapPanelDefaults.FALLBACK_CONTAINER_WIDTH;
        var height = containerHeight > 0 ? containerHeight : MapPanelDefaults.FALLBACK_CONTAINER_HEIGHT;

        var minX = MapPanelDefaults.MIN_VISIBLE_WIDTH - MapPanelDefaults.GetWidth(panel.Id);
        var maxX = width - MapPanelDefaults.MIN_VISIBLE_WIDTH;
        panel.X = Math.Clamp(x, Math.Min(minX, maxX), Math.Max(minX, maxX));
        panel.Y = Math.Clamp(y, 0, Math.Max(0, height - MapPanelDefaults.TITLE_BAR_HEIGHT));
    }

    private static MapLayoutDto ToDto(IReadOnlyList<MapPanelLayout> panels) =>
        new(CURRENT_LAYOUT_VERSION,
            panels.Select(panel => new MapPanelLayoutDto(
                panel.Id.ToString(),
                panel.X,
                panel.Y,
                panel.IsUserVisible,
                panel.IsCollapsed)).ToList());

    private static IReadOnlyList<MapPanelLayout> Mutate(
        IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId, Action<MapPanelLayout> mutation)
    {
        var panel = panels.FirstOrDefault(p => p.Id == panelId);
        if (panel is not null)
        {
            mutation(panel);
        }

        return panels;
    }

    private PendingSave StartSave(int mapId, MapLayoutDto snapshot)
    {
        var cancellation = new CancellationTokenSource();
        var task = SaveAfterDelayAsync(mapId, snapshot, cancellation.Token);
        return new PendingSave(task, cancellation);
    }

    private async Task SaveAfterDelayAsync(int mapId, MapLayoutDto snapshot, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(LAYOUT_SAVE_DEBOUNCE_MS, cancellationToken);
            await _storage.SetAsync(mapId, snapshot);
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer change, or the map was reset.
        }
    }

    private sealed record PendingSave(Task Task, CancellationTokenSource Cancellation);
}

using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Services.MapLayout;

/// <summary>
/// Builds, mutates and persists the panel layout of a map.
/// </summary>
/// <remarks>Mutations only return the updated panels; persisting is up to <see cref="ScheduleSaveAsync"/>.</remarks>
public interface IMapLayoutService
{
    /// <summary>
    /// Builds the default layout: every panel visible, expanded and at its default position.
    /// </summary>
    IReadOnlyList<MapPanelLayout> CreateDefaultLayout(double containerWidth, double containerHeight);

    /// <summary>
    /// Loads the stored layout of a map, merged over the defaults.
    /// </summary>
    /// <returns>The defaults when nothing valid is stored; never empty.</returns>
    Task<IReadOnlyList<MapPanelLayout>> LoadAsync(int mapId, double containerWidth, double containerHeight);

    /// <summary>
    /// Merges a stored layout over the defaults, ignoring unknown panels and clamping positions into the map.
    /// </summary>
    /// <returns>The defaults when <paramref name="stored"/> is null or its version is unsupported.</returns>
    IReadOnlyList<MapPanelLayout> Merge(MapLayoutDto? stored, double containerWidth, double containerHeight);

    /// <summary>
    /// Moves a panel by an offset, clamped so its title bar stays reachable.
    /// </summary>
    /// <returns>The panels unchanged when the panel is unknown or the offset is not finite.</returns>
    IReadOnlyList<MapPanelLayout> MoveBy(
        IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId,
        double deltaX, double deltaY, double containerWidth, double containerHeight);

    /// <summary>
    /// Raises a panel above all the others.
    /// </summary>
    IReadOnlyList<MapPanelLayout> BringToFront(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId);

    /// <summary>
    /// Hides a panel until the user shows it again from the dock.
    /// </summary>
    IReadOnlyList<MapPanelLayout> Close(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId);

    /// <summary>
    /// Flips the user visibility of a panel.
    /// </summary>
    IReadOnlyList<MapPanelLayout> ToggleVisibility(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId);

    /// <summary>
    /// Flips the collapsed state of a panel.
    /// </summary>
    IReadOnlyList<MapPanelLayout> ToggleCollapse(IReadOnlyList<MapPanelLayout> panels, MapPanelId panelId);

    /// <summary>
    /// Queues a debounced write of the layout, replacing any pending write for the same map.
    /// </summary>
    Task ScheduleSaveAsync(int mapId, IReadOnlyList<MapPanelLayout> panels);

    /// <summary>
    /// Awaits the pending write of a map, if any.
    /// </summary>
    Task FlushAsync(int mapId);

    /// <summary>
    /// Discards the stored layout of a map and returns the defaults.
    /// </summary>
    Task<IReadOnlyList<MapPanelLayout>> ResetAsync(int mapId, double containerWidth, double containerHeight);
}

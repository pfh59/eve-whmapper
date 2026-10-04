using System.Collections.Frozen;

namespace WHMapper.Models.DTO.MapLayout;

/// <summary>
/// Default width, size and position of each map panel, and the bounds a panel must respect.
/// </summary>
public static class MapPanelDefaults
{
    /// <summary>Horizontal slice of a panel that must stay inside the map so it can be grabbed back.</summary>
    public const double MIN_VISIBLE_WIDTH = 80;

    /// <summary>Height of the title bar, kept inside the map at the bottom edge.</summary>
    public const double TITLE_BAR_HEIGHT = 36;

    /// <summary>Map size assumed until the browser size is known.</summary>
    public const double FALLBACK_CONTAINER_WIDTH = 1600;
    public const double FALLBACK_CONTAINER_HEIGHT = 900;

    /// <summary>Smallest size a resizable panel can be shrunk to.</summary>
    public const double MIN_RESIZABLE_WIDTH = 480;
    public const double MIN_RESIZABLE_HEIGHT = 120;

    private const double EDGE_MARGIN = 8;
    private const double DEFAULT_WIDTH = 320;
    private const double DEFAULT_HEIGHT = 200;

    /// <summary>Widths sized so the content needs no horizontal scrollbar.</summary>
    private static readonly FrozenDictionary<MapPanelId, double> WIDTHS = new Dictionary<MapPanelId, double>
    {
        [MapPanelId.SystemInfos] = 320,
        [MapPanelId.Notes] = 320,
        [MapPanelId.Signatures] = 1060,
        [MapPanelId.RoutePlanner] = 340,
        [MapPanelId.LinkInfos] = 780
    }.ToFrozenDictionary();

    /// <summary>Default body heights of the panels the user may resize; the others size to their content.</summary>
    private static readonly FrozenDictionary<MapPanelId, double> RESIZABLE_BODY_HEIGHTS = new Dictionary<MapPanelId, double>
    {
        [MapPanelId.Signatures] = 325
    }.ToFrozenDictionary();

    /// <summary>Approximate rendered heights, only used to anchor default positions to the bottom edge.</summary>
    private static readonly FrozenDictionary<MapPanelId, double> NOMINAL_HEIGHTS = new Dictionary<MapPanelId, double>
    {
        [MapPanelId.SystemInfos] = 260,
        [MapPanelId.Notes] = 140,
        [MapPanelId.Signatures] = 370,
        [MapPanelId.RoutePlanner] = 370,
        [MapPanelId.LinkInfos] = 370
    }.ToFrozenDictionary();

    /// <summary>
    /// Gets the default width of a panel, in pixels.
    /// </summary>
    public static double GetWidth(MapPanelId panelId) =>
        WIDTHS.TryGetValue(panelId, out var width) ? width : DEFAULT_WIDTH;

    /// <summary>
    /// Gets the default size of a resizable panel.
    /// </summary>
    /// <returns><c>null</c> when the panel cannot be resized.</returns>
    public static MapPanelSize? GetDefaultSize(MapPanelId panelId) =>
        RESIZABLE_BODY_HEIGHTS.TryGetValue(panelId, out var height) ? new MapPanelSize(GetWidth(panelId), height) : null;

    /// <summary>
    /// Gets the default top-left position of a panel within a map of the given size.
    /// </summary>
    /// <remarks>Signatures and the connection panel share a corner: a system and a link are never selected together.</remarks>
    public static (double X, double Y) GetPosition(MapPanelId panelId, double containerWidth, double containerHeight)
    {
        var right = containerWidth - GetWidth(panelId) - EDGE_MARGIN;
        var bottom = containerHeight - GetNominalHeight(panelId) - EDGE_MARGIN;

        return panelId switch
        {
            MapPanelId.SystemInfos => (right, EDGE_MARGIN),
            MapPanelId.Notes => (right, EDGE_MARGIN + GetNominalHeight(MapPanelId.SystemInfos) + EDGE_MARGIN),
            MapPanelId.RoutePlanner => (right, bottom),
            MapPanelId.Signatures or MapPanelId.LinkInfos => (EDGE_MARGIN, bottom),
            _ => (EDGE_MARGIN, EDGE_MARGIN)
        };
    }

    private static double GetNominalHeight(MapPanelId panelId) =>
        NOMINAL_HEIGHTS.TryGetValue(panelId, out var height) ? height : DEFAULT_HEIGHT;
}

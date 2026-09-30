using MudBlazor;
using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Components.Pages.Mapper.Map.Panels;

/// <summary>
/// Title, icon and unavailability hint of the map panels; kept here so the layout model stays free of MudBlazor.
/// </summary>
public static class MapPanelPresentation
{
    /// <summary>
    /// Title shown in the panel title bar.
    /// </summary>
    public static string GetTitle(MapPanelId panelId) => panelId switch
    {
        MapPanelId.SystemInfos => "System",
        MapPanelId.Notes => "Notes",
        MapPanelId.Signatures => "Signatures",
        MapPanelId.RoutePlanner => "Routes",
        MapPanelId.LinkInfos => "Connection",
        _ => string.Empty
    };

    /// <summary>
    /// Material icon shown in the panel title bar and in the dock toggle.
    /// </summary>
    public static string GetIcon(MapPanelId panelId) => panelId switch
    {
        MapPanelId.SystemInfos => Icons.Material.Filled.Public,
        MapPanelId.Notes => Icons.Material.Filled.EditNote,
        MapPanelId.Signatures => Icons.Material.Filled.Radar,
        MapPanelId.RoutePlanner => Icons.Material.Filled.AltRoute,
        MapPanelId.LinkInfos => Icons.Material.Filled.Link,
        _ => Icons.Material.Filled.QuestionMark
    };

    /// <summary>
    /// Tooltip explaining why a panel cannot be shown with the current selection.
    /// </summary>
    public static string GetUnavailableHint(MapPanelId panelId) => panelId switch
    {
        MapPanelId.LinkInfos => "Select a connection",
        _ => "Select a system"
    };
}

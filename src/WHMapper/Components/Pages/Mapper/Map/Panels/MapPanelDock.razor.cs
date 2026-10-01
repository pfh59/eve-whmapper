using Microsoft.AspNetCore.Components;
using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Components.Pages.Mapper.Map.Panels;

/// <summary>
/// Toolbar pinned over the map, used to show and hide the panels.
/// </summary>
/// <remarks>A panel not applicable to the selection keeps a disabled toggle instead of disappearing.</remarks>
public partial class MapPanelDock
{
    /// <summary>Panels to expose, in dock order.</summary>
    [Parameter]
    public IReadOnlyList<MapPanelLayout> Panels { get; set; } = [];

    /// <summary>Tells whether a panel is relevant to the current selection.</summary>
    [Parameter, EditorRequired]
    public required Func<MapPanelId, bool> IsApplicable { get; set; }

    /// <summary>Raised when the user shows or hides a panel.</summary>
    [Parameter]
    public EventCallback<MapPanelId> OnToggle { get; set; }

    /// <summary>Raised when the user restores the default layout.</summary>
    [Parameter]
    public EventCallback OnResetLayout { get; set; }

    private static string BuildTooltip(MapPanelLayout panel, bool isApplicable)
    {
        var title = MapPanelPresentation.GetTitle(panel.Id);

        if (!isApplicable)
        {
            return $"{title} — {MapPanelPresentation.GetUnavailableHint(panel.Id)}";
        }

        return panel.IsUserVisible ? $"Hide {title}" : $"Show {title}";
    }
}

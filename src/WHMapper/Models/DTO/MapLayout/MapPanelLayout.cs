namespace WHMapper.Models.DTO.MapLayout;

/// <summary>
/// View state of one map panel.
/// </summary>
public sealed class MapPanelLayout
{
    /// <summary>Panel this state belongs to.</summary>
    public required MapPanelId Id { get; init; }

    /// <summary>Distance in pixels from the left edge of the map.</summary>
    public double X { get; set; }

    /// <summary>Distance in pixels from the top edge of the map.</summary>
    public double Y { get; set; }

    /// <summary>Whether the user wants the panel shown; it also needs to apply to the selection.</summary>
    public bool IsUserVisible { get; set; } = true;

    /// <summary>Whether only the title bar is shown.</summary>
    public bool IsCollapsed { get; set; }

    /// <summary>Stacking order, raised when the panel is activated. Not persisted.</summary>
    public int ZOrder { get; set; }
}

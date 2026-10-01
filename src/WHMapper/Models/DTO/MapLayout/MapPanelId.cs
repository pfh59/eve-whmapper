namespace WHMapper.Models.DTO.MapLayout;

/// <summary>
/// Identifies a map panel.
/// </summary>
public enum MapPanelId
{
    /// <summary>Solar system details (region, class, statics, effect).</summary>
    SystemInfos = 0,

    /// <summary>Free-text notes attached to the selected system.</summary>
    Notes = 1,

    /// <summary>Cosmic signatures of the selected system.</summary>
    Signatures = 2,

    /// <summary>Route planner for the selected system.</summary>
    RoutePlanner = 3,

    /// <summary>Connection details and jump log of the selected link.</summary>
    LinkInfos = 4
}

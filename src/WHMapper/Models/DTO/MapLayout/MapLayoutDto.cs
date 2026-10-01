namespace WHMapper.Models.DTO.MapLayout;

/// <summary>
/// Map panel layout as stored in browser local storage.
/// </summary>
/// <param name="Version">Schema version; any other version falls back to defaults.</param>
/// <param name="Panels">Per-panel state; unknown entries are ignored on read.</param>
public sealed record MapLayoutDto(int Version, IReadOnlyList<MapPanelLayoutDto> Panels);

/// <summary>
/// Stored state of a single panel.
/// </summary>
/// <remarks>The id is a string so that a renamed panel is ignored instead of breaking the whole layout.</remarks>
public sealed record MapPanelLayoutDto(
    string PanelId,
    double X,
    double Y,
    bool Visible,
    bool Collapsed);

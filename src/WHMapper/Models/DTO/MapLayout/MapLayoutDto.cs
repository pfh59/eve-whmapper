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
/// <param name="Width">Chosen width of a resizable panel; <c>null</c> otherwise or in older payloads.</param>
/// <param name="Height">Chosen body height of a resizable panel; <c>null</c> otherwise or in older payloads.</param>
public sealed record MapPanelLayoutDto(
    string PanelId,
    double X,
    double Y,
    bool Visible,
    bool Collapsed,
    double? Width = null,
    double? Height = null);

namespace WHMapper.Models.DTO.MapLayout;

/// <summary>
/// Size of a resizable map panel, in pixels.
/// </summary>
/// <param name="Width">Width of the whole panel.</param>
/// <param name="Height">Height of the body, below the title bar.</param>
public readonly record struct MapPanelSize(double Width, double Height);

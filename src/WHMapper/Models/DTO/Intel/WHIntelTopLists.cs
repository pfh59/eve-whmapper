namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// zKillboard top lists of a system, for the period defined by zKillboard.
/// </summary>
public sealed record WHIntelTopLists(
    IReadOnlyList<WHIntelTopEntry> Characters,
    IReadOnlyList<WHIntelTopEntry> Corporations,
    IReadOnlyList<WHIntelTopEntry> Alliances)
{
    public static WHIntelTopLists Empty { get; } = new([], [], []);
}

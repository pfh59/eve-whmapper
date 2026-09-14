namespace WHMapper.Models.DTO.Statistics;

/// <summary>
/// Top Probers and Top Explorers leaderboards for one scope and period.
/// </summary>
/// <param name="Probers">Characters ranked by signatures created and updated.</param>
/// <param name="Explorers">Characters ranked by systems opened.</param>
public record WHStatisticsReport(IReadOnlyList<WHProberRank> Probers, IReadOnlyList<WHExplorerRank> Explorers)
{
    /// <summary>
    /// Report without any ranked character.
    /// </summary>
    public static WHStatisticsReport Empty { get; } = new(Array.Empty<WHProberRank>(), Array.Empty<WHExplorerRank>());
}

namespace WHMapper.Models.DTO.Statistics;

/// <summary>
/// Period over which the leaderboards count activities.
/// </summary>
public enum WHStatisticsPeriod
{
    /// <summary>
    /// Activities recorded during the last 7 days.
    /// </summary>
    Last7Days,

    /// <summary>
    /// Activities recorded during the last 30 days.
    /// </summary>
    Last30Days,

    /// <summary>
    /// Every activity recorded in the activity log.
    /// </summary>
    AllTime
}

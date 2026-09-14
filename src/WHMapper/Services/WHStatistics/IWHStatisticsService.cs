using WHMapper.Models.Db;
using WHMapper.Models.DTO.Statistics;

namespace WHMapper.Services.WHStatistics;

/// <summary>
/// Builds the Top Probers and Top Explorers leaderboards from the activity log.
/// </summary>
/// <remarks>
/// Every method returns only data the viewer is authorized to see: instances the viewer can access, and maps
/// the viewer can open, including per-map access restrictions.
/// </remarks>
public interface IWHStatisticsService
{
    /// <summary>
    /// Instances the viewer can access, ordered by name.
    /// </summary>
    /// <param name="viewerCharacterId">Character viewing the statistics.</param>
    Task<IReadOnlyList<WHInstance>> GetAccessibleInstancesAsync(int viewerCharacterId);

    /// <summary>
    /// Maps of an instance the viewer can open, ordered by name.
    /// </summary>
    /// <param name="viewerCharacterId">Character viewing the statistics.</param>
    /// <param name="instanceId">Instance whose maps are listed.</param>
    /// <returns>The accessible maps; empty when the viewer cannot access the instance.</returns>
    Task<IReadOnlyList<WHMap>> GetAccessibleMapsAsync(int viewerCharacterId, int instanceId);

    /// <summary>
    /// Builds both leaderboards of an instance for every <see cref="WHStatisticsPeriod"/>.
    /// </summary>
    /// <param name="viewerCharacterId">Character viewing the statistics, used for authorization.</param>
    /// <param name="instanceId">Instance whose activities are counted.</param>
    /// <param name="mapId">Map the leaderboards are restricted to; null for every map the viewer can open.</param>
    /// <returns>
    /// One report per period; every report is <see cref="WHStatisticsReport.Empty"/> when the viewer cannot open
    /// any map of the requested scope.
    /// </returns>
    Task<IReadOnlyDictionary<WHStatisticsPeriod, WHStatisticsReport>> GetLeaderboardsAsync(int viewerCharacterId, int instanceId, int? mapId);
}

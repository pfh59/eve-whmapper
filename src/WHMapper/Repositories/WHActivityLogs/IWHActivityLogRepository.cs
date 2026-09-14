using WHMapper.Models.Db;
using WHMapper.Models.DTO.Statistics;

namespace WHMapper.Repositories.WHActivityLogs;

/// <summary>
/// Data access to the append-only activity log.
/// </summary>
public interface IWHActivityLogRepository : IDefaultRepository<WHActivityLog, int>
{
    /// <summary>
    /// Inserts several activities in a single database transaction.
    /// </summary>
    /// <returns>True when every activity was inserted; false when the insert failed.</returns>
    Task<bool> CreateRange(IEnumerable<WHActivityLog> activities);

    /// <summary>
    /// Counts activities per character and activity type.
    /// </summary>
    /// <param name="instanceId">Instance the activities belong to.</param>
    /// <param name="mapIds">Maps to include; activities not tied to one of these maps are ignored.</param>
    /// <param name="activityTypeIds">Activity types to include.</param>
    /// <param name="fromUtc">Inclusive lower bound of the activity date; null to include every retained activity.</param>
    /// <returns>One entry per character and activity type with at least one activity; empty on failure.</returns>
    Task<IReadOnlyList<WHActivityCount>> GetCountsByCharacterAsync(int instanceId, IReadOnlyCollection<int> mapIds, IReadOnlyCollection<int> activityTypeIds, DateTime? fromUtc);
}

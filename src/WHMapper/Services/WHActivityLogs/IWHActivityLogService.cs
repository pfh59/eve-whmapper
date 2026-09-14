using WHMapper.Models.DTO.EveMapper;

namespace WHMapper.Services.WHActivityLogs;

/// <summary>
/// Records activities performed by characters and applies the retention of the activity log.
/// </summary>
public interface IWHActivityLogService
{
    /// <summary>
    /// Number of days an activity is kept before being purged; 0 or less keeps activities indefinitely.
    /// </summary>
    int RetentionDays { get; }

    /// <summary>
    /// Records one or more activities of the same activity type, performed by a character on a map.
    /// </summary>
    /// <remarks>
    /// Never throws: a failure is logged, so the operation that triggered the activity is never interrupted.
    /// </remarks>
    /// <param name="characterId">EVE character that performed the activity.</param>
    /// <param name="activityTypeId">Activity type, see <see cref="WHMapper.Models.Db.WHActivityTypeIds"/>.</param>
    /// <param name="mapId">Map on which the activity was performed.</param>
    /// <param name="occurrences">Number of activities to record; nothing is recorded when 0 or less.</param>
    Task RecordAsync(int characterId, int activityTypeId, int mapId, int occurrences = 1);

    /// <summary>
    /// Records the signatures created and changed by a scan result import, from scan lines at 100% signal strength only.
    /// </summary>
    /// <remarks>
    /// Never throws, like <see cref="RecordAsync"/>.
    /// </remarks>
    /// <param name="characterId">EVE character that imported the scan result.</param>
    /// <param name="mapId">Map of the scanned system.</param>
    /// <param name="importResult">Outcome of the import.</param>
    Task RecordSignatureImportAsync(int characterId, int mapId, WHSignatureImportResult importResult);

    /// <summary>
    /// Deletes activities older than <see cref="RetentionDays"/>.
    /// </summary>
    /// <returns>Number of deleted activities; 0 when retention is disabled.</returns>
    Task<int> PurgeExpiredAsync();
}

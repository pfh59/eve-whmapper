namespace WHMapper.Models.DTO.Statistics;

/// <summary>
/// Number of activities of one activity type recorded for one character.
/// </summary>
/// <param name="CharacterId">EVE character that performed the activities.</param>
/// <param name="ActivityTypeId">Activity type, see <see cref="WHMapper.Models.Db.WHActivityTypeIds"/>.</param>
/// <param name="Count">Number of recorded activities.</param>
public record WHActivityCount(int CharacterId, int ActivityTypeId, int Count);

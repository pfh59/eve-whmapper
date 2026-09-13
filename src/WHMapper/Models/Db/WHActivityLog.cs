using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db;

/// <summary>
/// One activity performed by a character, recorded in the append-only activity log.
/// </summary>
/// <remarks>
/// The instance and map identifiers are stored without foreign keys, so deleting a map or a system never
/// deletes the recorded history. Both are null for an activity that is not tied to a map.
/// </remarks>
public class WHActivityLog
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// EVE character that performed the activity.
    /// </summary>
    [Required]
    public int CharacterId { get; set; }

    /// <summary>
    /// Activity type, referencing <see cref="WHActivityType"/>.
    /// </summary>
    [Required]
    public int WHActivityTypeId { get; set; }

    /// <summary>
    /// UTC date and time at which the activity was recorded.
    /// </summary>
    [Required]
    public DateTime ActivityDate { get; set; }

    /// <summary>
    /// Instance the map belonged to when the activity was recorded; null for an activity not tied to a map.
    /// </summary>
    public int? WHInstanceId { get; set; }

    /// <summary>
    /// Map on which the activity was performed; null for an activity not tied to a map.
    /// </summary>
    public int? WHMapId { get; set; }

    [Obsolete("EF Requires it")]
    protected WHActivityLog() { }

    public WHActivityLog(int characterId, int activityTypeId, int? instanceId, int? mapId)
    {
        CharacterId = characterId;
        WHActivityTypeId = activityTypeId;
        WHInstanceId = instanceId;
        WHMapId = mapId;
        ActivityDate = DateTime.UtcNow;
    }
}

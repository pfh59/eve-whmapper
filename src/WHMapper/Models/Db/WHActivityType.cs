using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db;

/// <summary>
/// Reference data describing a kind of activity that can be recorded in the activity log.
/// </summary>
/// <remarks>
/// A new kind of activity is added as a new row, without any change to the activity log schema.
/// </remarks>
public class WHActivityType
{
    /// <summary>
    /// Explicit identifier, identical across deployments. See <see cref="WHActivityTypeIds"/>.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique technical code of the activity type, such as <c>SignatureCreated</c>.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label of the activity type.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// False when the activity type is no longer recorded.
    /// </summary>
    /// <remarks>
    /// An activity type is deactivated rather than deleted, because recorded activities keep referencing it.
    /// </remarks>
    public bool IsActive { get; set; } = true;

    [Obsolete("EF Requires it")]
    protected WHActivityType() { }

    public WHActivityType(int id, string code, string label)
    {
        Id = id;
        Code = code;
        Label = label;
    }
}

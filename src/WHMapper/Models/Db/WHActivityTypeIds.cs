namespace WHMapper.Models.Db;

/// <summary>
/// Identifiers of the activity types seeded in the <c>ActivityTypes</c> table.
/// </summary>
/// <remarks>
/// Values must match the rows seeded in <see cref="WHMapper.Data.WHMapperContext"/>.
/// </remarks>
public static class WHActivityTypeIds
{
    /// <summary>
    /// A signature was added to a system.
    /// </summary>
    public const int SignatureCreated = 1;

    /// <summary>
    /// The name, group or type of an existing signature changed.
    /// </summary>
    public const int SignatureUpdated = 2;

    /// <summary>
    /// A system that was not on the map was added to it.
    /// </summary>
    public const int SystemOpened = 3;
}

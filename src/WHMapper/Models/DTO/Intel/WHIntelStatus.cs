namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// Outcome of an intel request.
/// </summary>
public enum WHIntelStatus
{
    /// <summary>zKillboard data was loaded and the system has recorded kills.</summary>
    Available,

    /// <summary>zKillboard has no recorded kill for the system.</summary>
    NoActivity,

    /// <summary>zKillboard could not be reached or rate limited the request.</summary>
    Unavailable,

    /// <summary>The viewer cannot open the map.</summary>
    Unauthorized
}

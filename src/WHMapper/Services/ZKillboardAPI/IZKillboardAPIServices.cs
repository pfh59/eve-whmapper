using WHMapper.Models.DTO.ZKillboard;

namespace WHMapper.Services.ZKillboardAPI;

/// <summary>
/// Read-only client of the public zKillboard API.
/// </summary>
public interface IZKillboardAPIServices
{
    /// <summary>
    /// Latest killmails of a solar system, newest first (one zKillboard page).
    /// </summary>
    /// <returns>The killmails, empty when the system has none; null when zKillboard fails or rate limits the request.</returns>
    Task<IReadOnlyList<ZKillmail>?> GetSystemKillmailsAsync(int solarSystemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Statistics of a solar system, including its top lists.
    /// </summary>
    /// <returns>The statistics; null when zKillboard fails or rate limits the request.</returns>
    Task<ZKillStats?> GetSystemStatsAsync(int solarSystemId, CancellationToken cancellationToken = default);
}

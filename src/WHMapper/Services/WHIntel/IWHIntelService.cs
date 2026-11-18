using WHMapper.Models.DTO.Intel;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Builds the System Intel of a solar system from zKillboard.
/// </summary>
/// <remarks>
/// Every method checks that the viewer can open the map; zKillboard is never called otherwise.
/// </remarks>
public interface IWHIntelService
{
    /// <summary>
    /// Loads the recent kills, top lists and analysis of a system. zKillboard data is cached and shared across users.
    /// </summary>
    /// <param name="viewerCharacterId">Character opening the intel, used for authorization.</param>
    /// <param name="mapId">Map the system is displayed on.</param>
    /// <returns>The report; its <see cref="WHIntelReport.Status"/> tells whether data is available.</returns>
    Task<WHIntelReport> GetIntelAsync(int viewerCharacterId, int mapId, int solarSystemId, string systemName, CancellationToken cancellationToken = default);

}

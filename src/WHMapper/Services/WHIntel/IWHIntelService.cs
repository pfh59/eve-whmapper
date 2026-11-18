using WHMapper.Models.DTO.Intel;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Builds the System Intel of a solar system from zKillboard, with an optional AI assessment.
/// </summary>
/// <remarks>
/// Every method checks that the viewer can open the map; zKillboard and the AI provider are never called otherwise.
/// </remarks>
public interface IWHIntelService
{
    /// <summary>
    /// True when an AI provider is configured.
    /// </summary>
    bool IsAiEnabled { get; }

    /// <summary>
    /// Loads the recent kills, top lists and analysis of a system. zKillboard data is cached and shared across users.
    /// </summary>
    /// <param name="viewerCharacterId">Character opening the intel, used for authorization.</param>
    /// <param name="mapId">Map the system is displayed on.</param>
    /// <returns>The report; its <see cref="WHIntelReport.Status"/> tells whether data is available.</returns>
    Task<WHIntelReport> GetIntelAsync(int viewerCharacterId, int mapId, int solarSystemId, string systemName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the AI assessment of a report, generating it when no assessment younger than one hour is cached.
    /// </summary>
    /// <returns>The assessment; its status explains why no text is available.</returns>
    Task<WHIntelAiReport> GetAiReportAsync(int viewerCharacterId, int mapId, WHIntelReport report, CancellationToken cancellationToken = default);
}

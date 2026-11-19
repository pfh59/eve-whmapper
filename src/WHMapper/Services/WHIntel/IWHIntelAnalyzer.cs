using WHMapper.Models.DTO.Intel;
using WHMapper.Models.DTO.ZKillboard;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Derives residents, activity profile, timezone and fleet composition from the killmails of a system.
/// </summary>
public interface IWHIntelAnalyzer
{
    /// <summary>
    /// Analyses the killmails of one system.
    /// </summary>
    /// <returns>The analysis; null when fewer than <see cref="WHIntelAnalyzer.MIN_KILLS_FOR_ANALYSIS"/> killmails are given.</returns>
    WHIntelAnalysis? Analyze(IReadOnlyList<ZKillmail> killmails);
}

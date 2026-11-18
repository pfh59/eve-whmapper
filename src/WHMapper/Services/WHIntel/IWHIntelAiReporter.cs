using WHMapper.Models.DTO.Intel;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Writes a short, non-interactive assessment of a system from its intel report.
/// </summary>
public interface IWHIntelAiReporter
{
    /// <summary>
    /// False when no AI provider is configured; <see cref="GenerateAsync"/> then always returns null.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Asks the AI provider for an assessment with the sections of <see cref="WHIntelAiReport.SECTION_HEADINGS"/>.
    /// </summary>
    /// <returns>The report text; null when the call fails, is refused or returns no text.</returns>
    Task<string?> GenerateAsync(WHIntelReport report, CancellationToken cancellationToken = default);
}

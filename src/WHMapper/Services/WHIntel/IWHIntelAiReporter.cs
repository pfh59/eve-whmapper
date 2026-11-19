using WHMapper.Models.DTO.Intel;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Writes a short, non-interactive assessment of a system from its intel report.
/// </summary>
public interface IWHIntelAiReporter
{
    /// <summary>
    /// False when no AI model is configured; <see cref="GenerateAsync"/> then always returns null.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Asks the AI model for an assessment written in the given language.
    /// </summary>
    /// <param name="languageTag">BCP 47 language tag of the viewer, e.g. <c>fr-FR</c>.</param>
    /// <returns>The report text; null when the call fails, is refused or returns no text.</returns>
    Task<string?> GenerateAsync(WHIntelReport report, string languageTag, CancellationToken cancellationToken = default);
}

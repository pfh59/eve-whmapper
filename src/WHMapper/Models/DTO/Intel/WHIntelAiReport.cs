namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// AI-generated assessment of a system.
/// </summary>
/// <param name="Text">Report text, one line per section; null unless <see cref="Status"/> is <see cref="WHIntelAiStatus.Generated"/>.</param>
public sealed record WHIntelAiReport(WHIntelAiStatus Status, string? Text, DateTime? GeneratedAtUtc)
{
    private const char HEADING_SEPARATOR = ':';

    /// <summary>
    /// Longer prefixes are part of a sentence, not a heading.
    /// </summary>
    private const int MAX_HEADING_LENGTH = 40;

    public static WHIntelAiReport WithStatus(WHIntelAiStatus status) => new(status, null, null);

    /// <summary>
    /// Splits <see cref="Text"/> into its non-empty lines, separating a leading "Heading:" from its text.
    /// </summary>
    /// <remarks>Headings are matched by position, not by name, because the report is written in the viewer's language.</remarks>
    public IReadOnlyList<WHIntelAiSection> GetSections() =>
        (Text ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ToSection)
            .ToList();

    private static WHIntelAiSection ToSection(string line)
    {
        int separator = line.IndexOf(HEADING_SEPARATOR);
        return separator is > 0 and <= MAX_HEADING_LENGTH
            ? new WHIntelAiSection(line[..separator].Trim(), line[(separator + 1)..].Trim())
            : new WHIntelAiSection(null, line);
    }
}

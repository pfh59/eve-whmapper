namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// AI-generated assessment of a system.
/// </summary>
/// <param name="Text">Report text, one line per section; null unless <see cref="Status"/> is <see cref="WHIntelAiStatus.Generated"/>.</param>
public sealed record WHIntelAiReport(WHIntelAiStatus Status, string? Text, DateTime? GeneratedAtUtc)
{
    private const string HEADING_SEPARATOR = ":";

    /// <summary>
    /// Section headings of a report, in display order.
    /// </summary>
    public static readonly IReadOnlyList<string> SECTION_HEADINGS =
    [
        "Residents",
        "Activity profile",
        "Active timezone",
        "Likely fleet composition",
        "Threat assessment"
    ];

    public static WHIntelAiReport WithStatus(WHIntelAiStatus status) => new(status, null, null);

    /// <summary>
    /// Splits <see cref="Text"/> into its non-empty lines, separating the known headings from their text.
    /// </summary>
    public IReadOnlyList<WHIntelAiSection> GetSections() =>
        (Text ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ToSection)
            .ToList();

    private static WHIntelAiSection ToSection(string line)
    {
        string? heading = SECTION_HEADINGS.FirstOrDefault(h => line.StartsWith(h + HEADING_SEPARATOR, StringComparison.OrdinalIgnoreCase));
        return heading == null
            ? new WHIntelAiSection(null, line)
            : new WHIntelAiSection(heading, line[(heading.Length + HEADING_SEPARATOR.Length)..].Trim());
    }
}

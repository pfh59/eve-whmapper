namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// One line of an AI report; <see cref="Heading"/> is null when the line does not start with a known heading.
/// </summary>
public sealed record WHIntelAiSection(string? Heading, string Text);

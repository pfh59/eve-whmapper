namespace WHMapper.Services.WHIntel;

/// <summary>
/// Settings of the System Intel feature, bound to the <c>Intel</c> configuration section.
/// </summary>
public sealed class WHIntelOptions
{
    public const string SECTION_NAME = "Intel";

    public WHIntelAiOptions Ai { get; set; } = new();
}

/// <summary>
/// AI provider used to write the intel report.
/// </summary>
public sealed class WHIntelAiOptions
{
    public const string PROVIDER_ANTHROPIC = "Anthropic";
    public const string DEFAULT_MODEL = "claude-sonnet-5-5";

    public string Provider { get; set; } = PROVIDER_ANTHROPIC;

    public string Model { get; set; } = DEFAULT_MODEL;

    /// <summary>
    /// Provider API key. Never committed: set it through <c>Intel__Ai__ApiKey</c> or a secret.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The AI report is generated only when an API key is set.
    /// </summary>
    public bool IsEnabled => !string.IsNullOrWhiteSpace(ApiKey);
}

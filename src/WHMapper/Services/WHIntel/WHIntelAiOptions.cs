namespace WHMapper.Services.WHIntel;

/// <summary>
/// AI model writing the intel report, bound to the <c>Intel:Ai</c> configuration section.
/// </summary>
/// <remarks>
/// Any OpenAI-compatible chat API works: a cloud provider (OpenAI, Mistral, Anthropic, OpenRouter…) or a local
/// container (Ollama, vLLM, LM Studio…).
/// </remarks>
public sealed class WHIntelAiOptions
{
    public const string SECTION_NAME = "Intel:Ai";
    public const int DEFAULT_TIMEOUT_SECONDS = 120;

    /// <summary>
    /// Base URL of the OpenAI-compatible API, e.g. <c>http://ollama:11434/v1</c>.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Model name as known by the endpoint, e.g. <c>qwen3:14b</c>.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// API key; empty for local servers that do not need one. Never committed: set <c>Intel__Ai__ApiKey</c>.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Local models can take minutes on a CPU.
    /// </summary>
    public int TimeoutSeconds { get; set; } = DEFAULT_TIMEOUT_SECONDS;

    /// <summary>
    /// The AI report is generated only when an endpoint and a model are set.
    /// </summary>
    public bool IsEnabled => !string.IsNullOrWhiteSpace(Endpoint) && !string.IsNullOrWhiteSpace(Model);
}

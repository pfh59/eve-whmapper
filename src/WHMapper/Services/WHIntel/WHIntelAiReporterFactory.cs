using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Builds the <see cref="IWHIntelAiReporter"/> of the configured OpenAI-compatible endpoint.
/// </summary>
public static class WHIntelAiReporterFactory
{
    /// <summary>
    /// The OpenAI client requires a key; local servers ignore it.
    /// </summary>
    private const string NO_API_KEY = "not-required";

    /// <summary>
    /// Returns a disabled reporter when the AI is not configured or the endpoint is not a valid URL.
    /// </summary>
    public static IWHIntelAiReporter Create(WHIntelAiOptions options, ILoggerFactory loggerFactory)
    {
        if (!options.IsEnabled)
            return new DisabledWHIntelAiReporter();

        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint))
        {
            loggerFactory.CreateLogger(typeof(WHIntelAiReporterFactory)).LogWarning(
                "Intel AI endpoint {Endpoint} is not a valid URL; the AI report is disabled", options.Endpoint);
            return new DisabledWHIntelAiReporter();
        }

        var client = new OpenAIClient(
            new ApiKeyCredential(string.IsNullOrWhiteSpace(options.ApiKey) ? NO_API_KEY : options.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = endpoint,
                NetworkTimeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
            });

        return new WHIntelAiReporter(
            client.GetChatClient(options.Model).AsIChatClient(),
            loggerFactory.CreateLogger<WHIntelAiReporter>());
    }
}

using Anthropic;
using Microsoft.Extensions.AI;
using WHMapper.Services.WHIntel;

namespace WHMapper.Extensions;

public static class IntelExtensions
{
    /// <summary>
    /// Registers the System Intel services and, when an API key is configured, the AI provider of the report.
    /// </summary>
    public static IServiceCollection AddIntelServices(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(WHIntelOptions.SECTION_NAME);
        services.Configure<WHIntelOptions>(section);

        var options = section.Get<WHIntelOptions>() ?? new WHIntelOptions();

        services.AddSingleton<IWHIntelAnalyzer, WHIntelAnalyzer>();
        services.AddSingleton<IWHIntelAiReporter>(sp => CreateAiReporter(options.Ai, sp.GetRequiredService<ILoggerFactory>()));
        services.AddScoped<IWHIntelService, WHIntelService>();

        return services;
    }

    /// <summary>
    /// Builds the reporter of the configured provider; an unsupported provider disables the AI report instead of failing startup.
    /// </summary>
    private static IWHIntelAiReporter CreateAiReporter(WHIntelAiOptions options, ILoggerFactory loggerFactory)
    {
        if (!options.IsEnabled)
            return new DisabledWHIntelAiReporter();

        IChatClient? chatClient = options.Provider switch
        {
            _ when string.Equals(options.Provider, WHIntelAiOptions.PROVIDER_ANTHROPIC, StringComparison.OrdinalIgnoreCase) =>
                new AnthropicClient { ApiKey = options.ApiKey }.AsIChatClient(options.Model),
            _ => null
        };

        if (chatClient == null)
        {
            loggerFactory.CreateLogger(typeof(IntelExtensions)).LogWarning(
                "Intel AI provider {Provider} is not supported; the AI report is disabled", options.Provider);
            return new DisabledWHIntelAiReporter();
        }

        return new WHIntelAiReporter(chatClient, loggerFactory.CreateLogger<WHIntelAiReporter>());
    }
}

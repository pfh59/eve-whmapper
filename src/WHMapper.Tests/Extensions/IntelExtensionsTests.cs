using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WHMapper.Extensions;
using WHMapper.Services.WHIntel;

namespace WHMapper.Tests.Extensions;

public class IntelExtensionsTests
{
    private static IWHIntelAiReporter ResolveReporter(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddIntelServices(configuration);

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IWHIntelAiReporter>();
    }

    [Fact]
    public void AddIntelServices_WithoutApiKey_DisablesAiReport()
    {
        var reporter = ResolveReporter(new() { ["Intel:Ai:Provider"] = "Anthropic", ["Intel:Ai:ApiKey"] = "" });

        Assert.IsType<DisabledWHIntelAiReporter>(reporter);
    }

    [Fact]
    public void AddIntelServices_WithAnthropicApiKey_EnablesAiReport()
    {
        var reporter = ResolveReporter(new() { ["Intel:Ai:Provider"] = "Anthropic", ["Intel:Ai:ApiKey"] = "test-key" });

        Assert.IsType<WHIntelAiReporter>(reporter);
    }

    [Fact]
    public void AddIntelServices_WithUnsupportedProvider_DisablesAiReport()
    {
        var reporter = ResolveReporter(new() { ["Intel:Ai:Provider"] = "Unknown", ["Intel:Ai:ApiKey"] = "test-key" });

        Assert.IsType<DisabledWHIntelAiReporter>(reporter);
    }

    [Fact]
    public void WHIntelOptions_DefaultsToAnthropicSonnet()
    {
        var options = new WHIntelOptions();

        Assert.Equal(WHIntelAiOptions.PROVIDER_ANTHROPIC, options.Ai.Provider);
        Assert.Equal("claude-sonnet-5-5", options.Ai.Model);
        Assert.False(options.Ai.IsEnabled);
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Services.WHIntel;

namespace WHMapper.Tests.Services.WHIntel;

public class WHIntelAiReporterFactoryTests
{
    [Theory]
    [InlineData("", "qwen3:14b")]
    [InlineData("http://ollama:11434/v1", "")]
    public void Create_WhenEndpointOrModelIsMissing_ReturnsDisabledReporter(string endpoint, string model)
    {
        var reporter = WHIntelAiReporterFactory.Create(new WHIntelAiOptions { Endpoint = endpoint, Model = model }, NullLoggerFactory.Instance);

        Assert.IsType<DisabledWHIntelAiReporter>(reporter);
    }

    [Fact]
    public void Create_WhenEndpointIsNotAUrl_ReturnsDisabledReporter()
    {
        var reporter = WHIntelAiReporterFactory.Create(new WHIntelAiOptions { Endpoint = "ollama", Model = "qwen3:14b" }, NullLoggerFactory.Instance);

        Assert.IsType<DisabledWHIntelAiReporter>(reporter);
    }

    [Theory]
    [InlineData("")]
    [InlineData("sk-test")]
    public void Create_WithEndpointAndModel_ReturnsEnabledReporterWithOrWithoutKey(string apiKey)
    {
        var options = new WHIntelAiOptions { Endpoint = "http://ollama:11434/v1", Model = "qwen3:14b", ApiKey = apiKey };

        var reporter = WHIntelAiReporterFactory.Create(options, NullLoggerFactory.Instance);

        Assert.IsType<WHIntelAiReporter>(reporter);
        Assert.True(reporter.IsEnabled);
    }

    [Fact]
    public void Options_AreDisabledByDefault()
    {
        var options = new WHIntelAiOptions();

        Assert.False(options.IsEnabled);
        Assert.Equal(WHIntelAiOptions.DEFAULT_TIMEOUT_SECONDS, options.TimeoutSeconds);
    }
}

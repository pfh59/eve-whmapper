using WHMapper.Models.DTO.Intel;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// <see cref="IWHIntelAiReporter"/> used when no AI model is configured.
/// </summary>
public sealed class DisabledWHIntelAiReporter : IWHIntelAiReporter
{
    public bool IsEnabled => false;

    public Task<string?> GenerateAsync(WHIntelReport report, string languageTag, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}

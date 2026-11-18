using WHMapper.Models.DTO.Intel;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// <see cref="IWHIntelAiReporter"/> used when no AI provider is configured.
/// </summary>
public sealed class DisabledWHIntelAiReporter : IWHIntelAiReporter
{
    public bool IsEnabled => false;

    public Task<string?> GenerateAsync(WHIntelReport report, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}

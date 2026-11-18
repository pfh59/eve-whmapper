using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WHMapper.Models.DTO.ZKillboard;

namespace WHMapper.Services.ZKillboardAPI;

/// <summary>
/// Default implementation of <see cref="IZKillboardAPIServices"/>.
/// </summary>
public class ZKillboardAPIServices : IZKillboardAPIServices
{
    /// <summary>
    /// zKillboard reports some errors with a 200 status and a JSON object holding this property.
    /// </summary>
    private const string ERROR_PROPERTY = "error";

    /// <summary>
    /// An empty body means "nothing recorded".
    /// </summary>
    private const string EMPTY_JSON_ARRAY = "[]";

    private static readonly JsonSerializerOptions JSON_OPTIONS = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<ZKillboardAPIServices> _logger;

    public ZKillboardAPIServices(HttpClient httpClient, ILogger<ZKillboardAPIServices> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        if (!httpClient.BaseAddress?.ToString().StartsWith(ZKillboardAPIServiceConstants.ZKillboardUrl, StringComparison.Ordinal) ?? true)
            throw new ArgumentException("HttpClient base address must start with the zKillboard URL.", nameof(httpClient));

        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ZKillmail>?> GetSystemKillmailsAsync(int solarSystemId, CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync(string.Format(CultureInfo.InvariantCulture, ZKillboardAPIServiceConstants.SYSTEM_KILLMAILS_PATH, solarSystemId), cancellationToken);
        if (document == null)
            return null;

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            _logger.LogWarning("zKillboard returned an unexpected killmail payload for system {SolarSystemId}", solarSystemId);
            return null;
        }

        return document.RootElement.Deserialize<List<ZKillmail>>(JSON_OPTIONS)?
            .OrderByDescending(x => x.KillmailTime)
            .ToList();
    }

    public async Task<ZKillStats?> GetSystemStatsAsync(int solarSystemId, CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync(string.Format(CultureInfo.InvariantCulture, ZKillboardAPIServiceConstants.SYSTEM_STATS_PATH, solarSystemId), cancellationToken);
        if (document == null)
            return null;

        // A system without any recorded kill has no statistics.
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            return new ZKillStats();

        return document.RootElement.Deserialize<ZKillStats>(JSON_OPTIONS);
    }

    /// <summary>
    /// Sends a GET request and parses the JSON body; null on HTTP failure, rate limit, invalid JSON or zKillboard error.
    /// The caller disposes the document.
    /// </summary>
    private async Task<JsonDocument?> GetJsonAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("zKillboard request {Uri} failed with status {StatusCode}", uri, (int)response.StatusCode);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? EMPTY_JSON_ARRAY : content);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty(ERROR_PROPERTY, out var error))
            {
                _logger.LogWarning("zKillboard request {Uri} returned the error {Error}", uri, error.ToString());
                document.Dispose();
                return null;
            }

            return document;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            _logger.LogWarning(ex, "zKillboard request {Uri} failed", uri);
            return null;
        }
    }
}

using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Services.MapLayout;

/// <summary>
/// Stores map panel layouts in the browser's local storage through <see cref="ProtectedLocalStorage"/>.
/// </summary>
/// <remarks>
/// Uses the framework-provided storage so that no custom JavaScript is required.
/// </remarks>
public sealed class ProtectedLocalStorageMapLayoutStorage : IMapLayoutStorage
{
    private const string MAP_LAYOUT_KEY_PREFIX = "whm.map-layout.v2.";

    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly ILogger<ProtectedLocalStorageMapLayoutStorage> _logger;

    public ProtectedLocalStorageMapLayoutStorage(
        ProtectedLocalStorage protectedLocalStorage,
        ILogger<ProtectedLocalStorageMapLayoutStorage> logger)
    {
        _protectedLocalStorage = protectedLocalStorage;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MapLayoutDto?> GetAsync(int mapId)
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<string>(BuildKey(mapId));
            if (!result.Success || string.IsNullOrWhiteSpace(result.Value))
            {
                return null;
            }

            return JsonSerializer.Deserialize<MapLayoutDto>(result.Value);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Corrupt stored layout for map {MapId}, falling back to defaults", mapId);
        }
        catch (CryptographicException ex)
        {
            // DataProtection keys live in Redis; a Redis flush makes every existing payload unreadable.
            _logger.LogWarning(ex, "Unprotectable stored layout for map {MapId}, falling back to defaults", mapId);
        }
        catch (JSDisconnectedException ex)
        {
            _logger.LogDebug(ex, "Circuit disconnected while reading the layout of map {MapId}", mapId);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task SetAsync(int mapId, MapLayoutDto layout)
    {
        try
        {
            await _protectedLocalStorage.SetAsync(BuildKey(mapId), JsonSerializer.Serialize(layout));
        }
        catch (JSDisconnectedException ex)
        {
            _logger.LogDebug(ex, "Circuit disconnected while saving the layout of map {MapId}", mapId);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(int mapId)
    {
        try
        {
            await _protectedLocalStorage.DeleteAsync(BuildKey(mapId));
        }
        catch (JSDisconnectedException ex)
        {
            _logger.LogDebug(ex, "Circuit disconnected while removing the layout of map {MapId}", mapId);
        }
    }

    private static string BuildKey(int mapId) => $"{MAP_LAYOUT_KEY_PREFIX}{mapId}";
}

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WHMapper.Services.Cache;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(ILogger<CacheService> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<T?> Get<T>(string key)
    {
        try
        {
            _logger.LogInformation($"Getting cache key {key}");
            var value = await _cache.GetStringAsync(key);
            if (value == null)
            {
                return default;       
            }

            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache key {key}");
            return default;
        }
    }

    public async Task<bool> Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        try
        {
            _logger.LogInformation($"Setting cache key {key}");
            var options = new DistributedCacheEntryOptions();
            if (absoluteExpirationRelativeToNow.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            }

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache key {key}");
            return false;
        }
    }

    public async Task<bool> Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogError("Error removing cache key: key is null or empty");
            return false;
        }
        try
        {
            _logger.LogInformation($"Removing cache key {key}");
            await _cache.RemoveAsync(key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache key {key}");
            return false;
        }
    }
}
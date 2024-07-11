namespace WHMapper.Services.Cache;

public interface ICacheService
{
    Task<T?> Get<T>(string key);
    Task<bool> Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
    Task<bool> Remove(string key);
}

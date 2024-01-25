namespace WHMapper.Services.Cache;

public interface ICacheService
{
    Task<T?> Get<T>(string key);
    Task Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
    Task Remove(string key);
}

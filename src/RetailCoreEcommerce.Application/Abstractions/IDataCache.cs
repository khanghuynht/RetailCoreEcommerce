namespace RetailCoreEcommerce.Application.Abstractions;

public interface IDataCache
{
    Task<string?> GetCacheAsync(string key);
    Task<T?> GetCacheAsync<T>(string key);
    Task<bool> SetCacheAsync(string key, object value, TimeSpan timeToLive, bool isSerialized = true);
    Task<bool> DeleteCacheAsync(string key);
}
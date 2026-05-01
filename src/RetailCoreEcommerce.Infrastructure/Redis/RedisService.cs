using System.Text.Json;
using RetailCoreEcommerce.Application.Abstractions;
using StackExchange.Redis;

namespace RetailCoreEcommerce.Infrastructure.Redis;

public class RedisService(IConnectionMultiplexer redis) : IDataCache
{
    private readonly IDatabase _redis = redis.GetDatabase();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<string?> GetCacheAsync(string key)
    {
        var data = await _redis.StringGetAsync(key);
        return data.IsNullOrEmpty ? null : data.ToString();
    }

    public async Task<T?> GetCacheAsync<T>(string key)
    {
        var data = await GetCacheAsync(key);
        return data == null ? default : JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);
    }

    public async Task<bool> SetCacheAsync(string key, object value, TimeSpan timeToLive, bool isSerialized = true)
    {
        
        if (!isSerialized)
        {
            return await _redis.StringSetAsync(key, value.ToString(), timeToLive);
        }

        var data = JsonSerializer.Serialize(value, _jsonSerializerOptions);
        return await _redis.StringSetAsync(key, data, timeToLive);
    }

    public Task<bool> DeleteCacheAsync(string key)
    {
        return _redis.KeyDeleteAsync(key);
    }
}
using System.Text.Json;
using DayClaim.AR.Application.Common.Interfaces;
using StackExchange.Redis;

namespace DayClaim.AR.Infrastructure.Caching;

/// <summary>Elasticache/Redis-compatible cache-aside layer (deck: "Cache-aside, Write-through (Elasticache/Redis)").</summary>
public class RedisCacheService(IConnectionMultiplexer connectionMultiplexer) : ICacheService
{
    private IDatabase Database => connectionMultiplexer.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await Database.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default) =>
        await Database.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        await Database.KeyDeleteAsync(key);
}

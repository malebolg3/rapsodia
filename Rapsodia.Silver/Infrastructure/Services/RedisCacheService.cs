using Rapsodia.Silver.Application.Interfaces;
using StackExchange.Redis;

namespace Rapsodia.Silver.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        => await _db.StringSetAsync(key, value, expiry);

    public async Task<string?> GetAsync(string key)
        => await _db.StringGetAsync(key);

    public async Task<bool> ExistsAsync(string key)
        => await _db.KeyExistsAsync(key);

    public async Task RemoveAsync(string key)
        => await _db.KeyDeleteAsync(key);
}

public class NullCacheService : ICacheService
{
    public Task SetAsync(string key, string value, TimeSpan? expiry = null) => Task.CompletedTask;
    public Task<string?> GetAsync(string key) => Task.FromResult<string?>(null);
    public Task<bool> ExistsAsync(string key) => Task.FromResult(false);
    public Task RemoveAsync(string key) => Task.CompletedTask;
}
using System.Text.Json;
using Data.Cache.Interfaces;
using StackExchange.Redis;

namespace Data.Cache.Models;

public sealed class RedisCacheClient : ICacheClient
{
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web);

    public RedisCacheClient(IConnectionMultiplexer mux) => _db = mux.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var val = await _db.StringGetAsync(key);
        return val.HasValue ? JsonSerializer.Deserialize<T>(val!, _opts) : default;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) =>
        _db.StringSetAsync(key, JsonSerializer.Serialize(value, _opts), ttl);

    public Task<bool> RemoveAsync(string key, CancellationToken ct = default) =>
        _db.KeyDeleteAsync(key);
}

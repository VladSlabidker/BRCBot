using Data.Cache.Configs;
using Data.Cache.Interfaces;
using Data.Cache.Models;
using StackExchange.Redis;

namespace Data.Cache.Extensions;

public static class RedisExtension
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration cfg)
    {
        var _redisConfig = cfg.GetSection(nameof(RedisConfig)).Get<RedisConfig>()
                           ?? throw new InvalidOperationException($"Section '{nameof(RedisConfig)}' missing");
        
        var connStr = $"{_redisConfig.Host}:{_redisConfig.Port},abortConnect={_redisConfig.AbortConnect.ToString().ToLower()}";
        if (!string.IsNullOrWhiteSpace(_redisConfig.Password))
            connStr += $",password={_redisConfig.Password}";
        if (_redisConfig.Database is not null)
            connStr += $",defaultDatabase={_redisConfig.Database}";
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connStr));

        services.AddScoped<ICacheClient, RedisCacheClient>();

        return services;
    }
}
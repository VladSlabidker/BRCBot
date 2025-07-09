namespace Data.Cache.Configs;

public class RedisConfig
{
    public string Host { get; init; }

    public int Port { get; init; }

    public string? Password { get; init; }

    public int? Database { get; init; }

    public bool AbortConnect { get; init; }
}
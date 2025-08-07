using Data.Cache.Interfaces;

namespace Data.Cache.Models;

public class RateLimiter
{
    private readonly ICacheClient _cacheClient;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    public RateLimiter(ICacheClient cacheClient, int maxRequests = 3, TimeSpan? window = null)
    {
        _cacheClient = cacheClient;
        _maxRequests = maxRequests;
        _window = window ?? TimeSpan.FromHours(24);
    }

    public async Task<bool> TryConsumeAsync(long telegramUserId, CancellationToken ct = default)
    {
        string key = $"telegram:user:{telegramUserId}:daily_requests";

        var count = await _cacheClient.GetAsync<int>(key, ct);
        if (count == 0)
        {
            // Первый запрос — записываем 1 с TTL
            await _cacheClient.SetAsync(key, 1, _window, ct);
            return true;
        }

        if (count < _maxRequests)
        {
            // Увеличиваем счетчик
            await _cacheClient.SetAsync(key, count + 1, _window, ct);
            return true;
        }

        // Превышен лимит
        return false;
    }
}

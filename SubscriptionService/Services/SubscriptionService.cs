using SubscriptionService.Interfaces;
using SubscriptionService.Models;

namespace SubscriptionService.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IBillingProvider _billing;

    public SubscriptionService(IBillingProvider billing)
    {
        _billing = billing;
    }

    public Task<bool> IsActiveAsync(long userId)
    {
        // заглушка — всегда true
        return Task.FromResult(true);
    }

    public Task<DateTime?> GetEndDateAsync(long userId)
    {
        return Task.FromResult<DateTime?>(DateTime.UtcNow.AddDays(30));
    }

    public async Task ProcessBillingAsync(SubscriptionBillingMessage message)
    {
        var result = await _billing.ChargeAsync(message.UserId, message.Amount, message.Type);

        Console.WriteLine($"[Billing] UserId: {message.UserId}, Success: {result.Success}");

        // Здесь можно сохранить в БД Payment / Subscription
    }
}
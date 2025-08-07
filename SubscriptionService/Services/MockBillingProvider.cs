using Common.Enums;
using SubscriptionService.Interfaces;
using SubscriptionService.Models;

namespace SubscriptionService.Services;

public class MockBillingProvider : IBillingProvider
{
    public Task<BillingResult> ChargeAsync(long userId, decimal amount, SubscriptionType type)
    {
        return Task.FromResult(new BillingResult
        {
            Success = true,
            ExternalTransactionId = Guid.NewGuid().ToString()
        });
    }
}

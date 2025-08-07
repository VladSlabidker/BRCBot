using Common.Enums;
using SubscriptionService.Models;

namespace SubscriptionService.Interfaces;

public interface IBillingProvider
{
    Task<BillingResult> ChargeAsync(long userId, decimal amount, SubscriptionType type);
}
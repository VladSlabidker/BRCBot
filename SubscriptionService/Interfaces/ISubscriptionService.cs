using SubscriptionService.Models;

namespace SubscriptionService.Interfaces;

public interface ISubscriptionService
{
    Task<bool> IsActiveAsync(long userId);
    
    Task<DateTime?> GetEndDateAsync(long userId);
    
    Task ProcessBillingAsync(SubscriptionBillingMessage message);
}
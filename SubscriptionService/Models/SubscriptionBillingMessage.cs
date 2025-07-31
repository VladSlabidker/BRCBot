using Common.Enums;

namespace SubscriptionService.Models;

public class SubscriptionBillingMessage
{
    public long UserId { get; set; }
    
    public decimal Amount { get; set; }
    
    public SubscriptionType Type { get; set; }
}
namespace SubscriptionService.Models;

public class BillingResult
{
    public bool Success { get; set; }
    
    public string? ExternalTransactionId { get; set; }
    
    public string? ErrorMessage { get; set; }
}
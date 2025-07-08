using Common.Enums;

namespace Data.SQL.Models;

public class Payment
{
    public long Id { get; set; }
    
    public long UserId { get; set; }
    
    public decimal Amount { get; set; }
    
    public PaymentStatus Status { get; set; }

    public User User { get; set; }
    
    public Subscription Subscription { get; set; }
}
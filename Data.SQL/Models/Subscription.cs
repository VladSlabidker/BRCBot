namespace Data.SQL.Models;

public class Subscription
{
    public long Id { get; set; }
    
    public long UserId { get; set; }
    
    public long? PaymentId { get; set; }
    
    public DateTime StartsAt { get; set; }
    
    public DateTime EndsAt { get; set; }
    
    public bool IsActive { get; set; }
    
    public User User { get; set; }
    
    public Payment Payment { get; set; }
}
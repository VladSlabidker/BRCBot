namespace Data.SQL.Models;

public class User
{
    public long Id { get; set; }
    
    public string Username { get; set; }
    
    public DateTime ActivatedAt { get; set; }
    
    public bool IsBlocked { get; set; }
    
    public ICollection<Receipt> Receipts { get; set; }
    
    public ICollection<Subscription> Subscriptions { get; set; }
    
    public ICollection<Payment> Payments { get; set; }
}
namespace Data.SQL.Models;

public class Receipt
{
    public long Id { get; set; }
    
    public long UserId { get; set; }
    
    public long BankId { get; set; }
    
    public string Code { get; set; }
    
    public decimal Amount { get; set; }
    
    public bool IsValid { get; set; }
    
    public byte LastFour { get; set; }
    
    public DateTime CheckedAt { get; set; }
    
    public User User { get; set; }
    
    public Bank Bank { get; set; }
}   
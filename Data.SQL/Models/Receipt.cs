namespace Data.SQL.Models;

public class Receipt
{
    public long Id { get; set; }
    
    public long BankId { get; set; }
    
    public string Code { get; set; }
    
    public double Amount { get; set; }
    
    public bool IsValid { get; set; }
    
    public int LastFour { get; set; }
    
    public string? Link { get; set; }
    
    public DateTime CheckedAt { get; set; }
    
    public Bank Bank { get; set; }
}   
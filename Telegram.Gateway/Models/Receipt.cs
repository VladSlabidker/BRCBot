using Google.Protobuf.WellKnownTypes;

namespace Telegram.Gateway.Models;

public class Receipt
{
    public long Id { get; set; }
    
    public long BankId { get; set; }
    
    public string Code { get; set; } = string.Empty;
    
    public double Amount { get; set; }
    
    public bool IsValid { get; set; }
    
    public Timestamp CheckedAt { get; set; }

    public string Link { get; set; } = string.Empty;
}
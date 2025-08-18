namespace OcrService.Models;

public class OcrRequest
{
    public string ImageBase64 { get; set; } = string.Empty;
    
    public Guid CorrelationId { get; set; }
}
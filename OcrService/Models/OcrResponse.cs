namespace OcrService.Models;

public class OcrResponse
{
    public string Text { get; set; } = string.Empty;
    
    public string Error { get; set; } = string.Empty;
    
    public Guid CorrelationId { get; set; }
}
using OcrService.Models;

namespace OcrService.Interfaces;

public interface IOcrService
{
    public Task<Receipt> GetReceiptFromImageAsync(string base64, CancellationToken cancellationToken);
}
using OcrService.Models;

namespace OcrService.Interfaces;

public interface IOcrService
{
    public Receipt GetReceiptFromImage(string base64);
}
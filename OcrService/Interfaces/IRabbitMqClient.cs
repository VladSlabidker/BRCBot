using OcrService.Models;

public interface IRabbitMqClient
{
    Task<OcrResponse> SendRequestAndWaitForResponseAsync(OcrRequest request, CancellationToken cancellationToken = default);
}
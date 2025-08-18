using MassTransit;
using OcrService.Models;

namespace OcrService.Services;

public class OcrResponseConsumer : IConsumer<OcrResponse>
{
    public async Task Consume(ConsumeContext<OcrResponse> context)
    {
        Console.WriteLine($"[OCR RESPONSE] Text: {context.Message.Text}");
        Console.WriteLine($"[OCR RESPONSE] Error: {context.Message.Error}");
        await Task.CompletedTask;
    }
}
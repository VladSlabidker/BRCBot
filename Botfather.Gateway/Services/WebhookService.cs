using Botfather.Gateway.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Botfather.Gateway.Services;

public class WebhookService: IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly IMessageService _messageService;

    public WebhookService(ILogger<WebhookService> logger, IMessageService messageService)
    {
        _logger = logger;
        _messageService = messageService;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Text is { } text)
        {
            await _messageService.SendMessageAsync(
                update.Message.Chat.Id,
                text,
                cancellationToken);
        }
    }
}
using Botfather.Gateway.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Botfather.Gateway.Services;

public class MessageService: IMessageService
{
    private ILogger<MessageService> _logger;
    private ITelegramBotClient _botClient;

    public MessageService(ILogger<MessageService> logger, ITelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }

    public async Task<Message> SendMessageAsync(long chatId, string text, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Sending message: {text.Length} characters");
        
        var message = await _botClient.SendTextMessageAsync(chatId, text, parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);
        
        _logger.LogInformation("Message sent");

        return message;
    }
}
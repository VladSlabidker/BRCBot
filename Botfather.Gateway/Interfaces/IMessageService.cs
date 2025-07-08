using Telegram.Bot.Types;

namespace Botfather.Gateway.Interfaces;

public interface IMessageService
{
    public Task<Message> SendMessageAsync(long chatId, string text, CancellationToken cancellationToken);
}
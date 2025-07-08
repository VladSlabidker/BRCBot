using Telegram.Bot.Types;

namespace Botfather.Gateway.Interfaces;

public interface IWebhookService
{
    public Task HandleAsync(Update update, CancellationToken cancellationToken);
}
using Telegram.Bot.Types;

namespace Telegram.Gateway.Interfaces;

public interface ITelegramUpdateService
{
    Task HandleUpdateAsync(Update update);
}
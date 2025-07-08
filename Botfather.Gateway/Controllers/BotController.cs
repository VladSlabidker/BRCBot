using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Botfather.Gateway.Controllers;

[ApiController]
[Route("api/bot")]
public class BotController: ControllerBase
{
    private ILogger<BotController> _logger;
    private ITelegramBotClient _botClient;
    
    public BotController(ILogger<BotController> logger, ITelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }
    
}
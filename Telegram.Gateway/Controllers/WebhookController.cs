using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Gateway.Interfaces;

namespace Telegram.Gateway.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ITelegramUpdateService _updateService;

    public WebhookController(ITelegramUpdateService updateService)
    {
        _updateService = updateService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        await _updateService.HandleUpdateAsync(update);
        return Ok();
    }
}
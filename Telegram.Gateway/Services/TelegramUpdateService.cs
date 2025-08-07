using System.Text.Json;
using Common.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Gateway.Interfaces;
using Telegram.Gateway.Models;

namespace Telegram.Gateway.Services;

public class TelegramUpdateService : ITelegramUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<TelegramUpdateService> _logger;

    // Храним: какой пользователь ждет чек
    private static readonly Dictionary<long, bool> _waitingForReceipt = new();

    public TelegramUpdateService(
        ITelegramBotClient botClient,
        IHttpClientFactory httpFactory,
        ILogger<TelegramUpdateService> logger)
    {
        _botClient = botClient;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update)
    {
        if (update.Type == UpdateType.Message)
        {
            var msg = update.Message!;
            var chatId = msg.Chat.Id;

            // Команда /start
            if (msg.Text == "/start")
            {
                await _botClient.SendMessage(chatId,
                    "Привет!",
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData("Проверить чек", "check_receipt")));
                return;
            }

            // Скриншот после кнопки
            if (msg.Photo != null && _waitingForReceipt.TryGetValue(chatId, out bool waiting) && waiting)
            {
                var bestPhoto = msg.Photo.OrderByDescending(p => p.FileSize).First();
                var file = await _botClient.GetFile(bestPhoto.FileId);

                using var memoryStream = new MemoryStream();
                await _botClient.DownloadFile(file.FilePath!, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var bytes = memoryStream.ToArray();

                try
                {
                    var client = _httpFactory.CreateClient("Storefront");
                    var content = JsonContent.Create(bytes);

                    var response = await client.PostAsync("api/validation", content);
                    var body = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<Receipt>(body, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result.IsValid)
                    {
                        var checkedAt = DateTimeOffset.FromUnixTimeSeconds(result.CheckedAt.Seconds).ToLocalTime();

                        var text = $"""
                                    ✅ Чек успешно распознан!

                                    📄 Код: {result.Code}
                                    💳 Сумма: {result.Amount:F2} грн
                                    🏦 Банк: {Enum.GetName((BankType)result.BankId)}
                                    📅 Проверен: {checkedAt:dd.MM.yyyy HH:mm:ss}
                                    🔗 [Посмотреть в банке]({result.Link})
                                    """;

                        await _botClient.SendMessage(
                            chatId,
                            text,
                            parseMode: ParseMode.Markdown
                        );
                    }
                    else
                    {
                        await _botClient.SendMessage(
                            chatId,
                            "❌ Чек НЕВАЛИДНЫЙ",
                            parseMode: ParseMode.Markdown
                        );  
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при отправке чека");
                    await _botClient.SendMessage(chatId, "Произошла ошибка при проверке чека.");
                }
                finally
                {
                    _waitingForReceipt.Remove(chatId);
                }

                return;
            }

            // Прочие сообщения
            await _botClient.SendMessage(chatId, "Я понимаю только команду /start или фото после кнопки.");
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            var callback = update.CallbackQuery!;
            var chatId = callback.Message!.Chat.Id;

            if (callback.Data == "check_receipt")
            {
                _waitingForReceipt[chatId] = true;

                await _botClient.SendMessage(chatId,
                    "Теперь пришли, пожалуйста, фото чека.");
            }
        }
    }
}

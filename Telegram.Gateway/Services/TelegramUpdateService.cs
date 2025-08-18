using System.Text.Json;
using Common.Enums;
using Common.Exceptions;
using Grpc.Core;
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
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "🧾 Проверить чек" },
                    new KeyboardButton[] { "⚠️ Сообщить об ошибке" }
                })
                {
                    ResizeKeyboard = true
                };

                await _botClient.SendMessage(chatId,
                    "Привет!",
                    replyMarkup: keyboard);
                return;
            }

            // Кнопка "Проверить чек"
            if (msg.Text == "🧾 Проверить чек")
            {
                _waitingForReceipt[chatId] = true;

                await _botClient.SendMessage(chatId,
                    "Теперь пришли, пожалуйста, фото чека.");
                return;
            }

            // Кнопка "Сообщить об ошибке"
            if (msg.Text == "⚠️ Сообщить об ошибке")
            {
                await _botClient.SendMessage(chatId,
                    "Напиши, пожалуйста, сюда \n👉 @slabidker",
                    parseMode: ParseMode.Markdown);
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
                                     
                                    По всем вопросам пишите @slabidker
                                    """;

                        await _botClient.SendMessage(
                            chatId,
                            text,
                            parseMode: ParseMode.Markdown
                        );
                    }
                }
                catch (InvalidReceiptException)
                {
                    await _botClient.SendMessage(
                        chatId,
                        "❌ Чек НЕВАЛИДНЫЙ",
                        parseMode: ParseMode.Markdown
                    );
                }
                catch (RpcException ex)
                {
                    _logger.LogError(ex, "Ошибка внутри сервиса");
                    await _botClient.SendMessage(chatId,
                        $"❌ Ошибка от сервиса: {ex.Status.Detail}. \nПо всем вопросам пишите: @slabidker");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при отправке чека");
                    await _botClient.SendMessage(chatId,
                        "❌ Произошла ошибка при проверке чека. \nПо всем вопросам пишите: @slabidker");
                }
                finally
                {
                    _waitingForReceipt.Remove(chatId);
                }

                return;
            }

            // Прочие сообщения
            await _botClient.SendMessage(chatId, "Неправильная команда, введите пожалуйста /start и следуйте инструкциям");
        }
    }
}
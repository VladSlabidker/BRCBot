using System.Text.RegularExpressions;
using Common.Enums;
using Microsoft.Extensions.Options;
using OcrService.Configs;
using OcrService.Interfaces;
using OcrService.Models;
using static OcrService.Services.BankReceiptParserService;

namespace OcrService.Services;

public class PaddleService : IOcrService
{
    private readonly IRabbitMqClient _client;
    private readonly RabbitMqConfig _rabbitMqConfig;

    public PaddleService(IRabbitMqClient client, IOptions<RabbitMqConfig> options)
    {
        _client = client;
        _rabbitMqConfig = options.Value;
    }

    public async Task<Receipt> GetReceiptFromImageAsync(string base64, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<Receipt>(cancellationToken);

        // Убираем префикс, если есть
        if (base64.Contains(","))
            base64 = base64.Split(',')[1];

        // Создаём запрос
        var request = new OcrRequest
        {
            ImageBase64 = base64
            // CorrelationId будет создан внутри RabbitMqClient
        };

        // Отправляем и ждём ответа
        var response = await _client.SendRequestAndWaitForResponseAsync(request, cancellationToken);

        string text = response.Text;
        
        if(string.IsNullOrWhiteSpace(text))
            throw new InvalidDataException($"Failed to preproccess the image: {response.Error}");
        
        // Логика определения банка (как в TesseractService)
        const string pattern = @"\b(?:Приватбанк|Універсал\s*банк|Банк\s*Власний\s*Рахунок|Банк\s*Восток|Восток\s*Банк|МТБ\s*Банк|MTB\s*Bank|monobank|Universal\s*Bank|privatbank)\b";
        var bankName = Regex.Match(text, pattern,
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        if (bankName.Success)
        {
            string name = NormalizeBankName(bankName.Value);

            if (_bankNameMap.TryGetValue(name, out var bankType))
                return ParseReceiptFromText(bankType, text);
            
            throw new InvalidDataException($"{name} is not a valid bank name");
        }
        throw new InvalidDataException($"Wrong image with length: {text.Length}");
    }

    private readonly Dictionary<string, BankType> _bankNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "приват", BankType.Privat24 },
        { "універсал", BankType.Mono },
        { "власнийрахунок", BankType.Bvr },
        { "восток", BankType.Vostok },
        { "мтб", BankType.Mtb },
        { "mtb", BankType.Mtb },
        { "mono", BankType.Mono },
        { "universal", BankType.Mono },
        { "privat", BankType.Mono },
    };

    private string NormalizeBankName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        raw = raw.ToLowerInvariant();
        raw = Regex.Replace(raw, @"\s+", " ").Trim();
        raw = Regex.Replace(raw, @"\bmonobank\b", "mono банк", RegexOptions.IgnoreCase);
        raw = Regex.Replace(raw, @"(приват|універсал|власний рахунок|восток|мтб|mtb|mono)(банк)", "$1 банк", RegexOptions.IgnoreCase);
        raw = Regex.Replace(raw, @"(банк)(приват|універсал|власний рахунок|восток|мтб|mtb|mono)", "$2 банк", RegexOptions.IgnoreCase);

        if (raw.StartsWith("банк "))
            raw = raw.Substring(5) + " банк";

        return raw.Replace(" банк", "").Replace(" ", "");
    }

    private Receipt ParseReceiptFromText(BankType bankType, string text) =>
        bankType switch
        {
            BankType.Mono => ParseMonoReceipt(bankType, text),
            BankType.Privat24 => ParsePrivat24Receipt(bankType, text),
            BankType.Vostok or BankType.Bvr => ParseVostokReceipt(bankType, text),
            BankType.Mtb => ParseMtbReceipt(bankType, text),
            _ => throw new InvalidDataException($"{bankType} is not a valid bank name")
        };
}

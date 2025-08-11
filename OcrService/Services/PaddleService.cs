using Common.Enums;
using MassTransit;
using OcrService.Interfaces;
using OcrService.Models;
using OcrService.Models;
using static OcrService.Services.BankReceiptParserService;

namespace OcrService.Services;

public class PaddleService : IOcrService
{
    private readonly IRequestClient<OcrRequest> _client;

    public PaddleService(IRequestClient<OcrRequest> client)
    {
        _client = client;
    }

    public async Task<Receipt> GetReceiptFromImageAsync(string base64, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<Receipt>(cancellationToken);

        // Убираем префикс, если есть
        if (base64.Contains(",")) 
            base64 = base64.Split(',')[1];

        // Отправляем запрос в Python через RabbitMQ
        var response = await _client.GetResponse<OcrResponse>(new OcrRequest
        {
            ImageBase64 = base64
        }, cancellationToken);

        string text = response.Message.Text;

        // Логика определения банка (та же, что в TesseractService)
        const string pattern = @"\b(?:Приватбанк|Універсал\s*банк|Банк\s*Власний\s*Рахунок|Банк\s*Восток|Восток\s*Банк|МТБ\s*Банк|MTB\s*Bank|monobank|Universal\s*Bank|privatbank)\b";
        var bankName = System.Text.RegularExpressions.Regex.Match(text, pattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        if (bankName.Success)
        {
            string name = NormalizeBankName(bankName.Value);

            if (_bankNameMap.TryGetValue(name, out var bankType))
                return ParseReceiptFromText(bankType, text);
            else
                throw new InvalidDataException($"{name} is not a valid bank name");
        }
        else
        {
            throw new InvalidDataException($"Wrong image with length: {text.Length}");
        }
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
        raw = System.Text.RegularExpressions.Regex.Replace(raw, @"\s+", " ").Trim();
        raw = System.Text.RegularExpressions.Regex.Replace(raw, @"\bmonobank\b", "mono банк", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        raw = System.Text.RegularExpressions.Regex.Replace(raw, @"(приват|універсал|власний рахунок|восток|мтб|mtb|mono)(банк)", "$1 банк", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        raw = System.Text.RegularExpressions.Regex.Replace(raw, @"(банк)(приват|універсал|власний рахунок|восток|мтб|mtb|mono)", "$2 банк", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

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

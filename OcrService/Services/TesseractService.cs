using System.Text.RegularExpressions;
using Common.Enums;
using OcrService.Interfaces;
using OcrService.Models;
using Tesseract;
using static OcrService.Services.BankReceiptParserService;

namespace OcrService.Services;

public class TesseractService: IOcrService
{
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
    
    private readonly TesseractEngine _tesseractEngine;

    public TesseractService(TesseractEngine tesseractEngine)
    {
        _tesseractEngine = tesseractEngine;
    }
    
    public async Task<Receipt> GetReceiptFromImageAsync(string base64, CancellationToken cancellationToken)
    {
        if(cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<Receipt>(cancellationToken).Result;
        
        // Очистка base64, если с префиксом
        if (base64.Contains(",")) base64 = base64.Split(',')[1];

        var imageBytes = Convert.FromBase64String(base64);
        using var pix = Pix.LoadFromMemory(imageBytes);
        using var page = _tesseractEngine.Process(pix);
        var text = page.GetText();

        const string pattern = @"\b(?:Приватбанк|Універсал\s*банк|Банк\s*Власний\s*Рахунок|Банк\s*Восток|Восток\s*Банк|МТБ\s*Банк|MTB\s*Bank|monobank|Universal\s*Bank|privatbank)\b";
        var bankName = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        
        if (bankName.Success)
        {
            string name = NormalizeBankName(bankName.Value);

            if (_bankNameMap.TryGetValue(name, out var bankType))
                return ParseReceiptFromText(bankType, text);
            else 
                throw new InvalidDataException($"{name} is not a valid bank name");
        }
        else 
            throw new InvalidDataException($"Wrong image with length: {text.Length}");
    }

    private string NormalizeBankName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        // Приводим к нижнему регистру и убираем лишние пробелы
        raw = raw.ToLowerInvariant();
        raw = Regex.Replace(raw, @"\s+", " ").Trim();
        
        raw = Regex.Replace(raw, @"\bmonobank\b", "mono банк", RegexOptions.IgnoreCase);

        // Разделяем слитные слова: приватбанк -> приват банк и банкприват -> приват банк
        raw = Regex.Replace(raw, @"(приват|універсал|власний рахунок|восток|мтб|mtb|mono)(банк)", "$1 банк", RegexOptions.IgnoreCase);
        raw = Regex.Replace(raw, @"(банк)(приват|універсал|власний рахунок|восток|мтб|mtb|mono)", "$2 банк", RegexOptions.IgnoreCase);

        // Переставляем "банк" вправо, если он в начале
        if (raw.StartsWith("банк "))
            raw = raw.Substring(5) + " банк";

        // Возвращаем часть без "банк" и без пробелов - например, "приват", "власнийрахунок"
        string cleanName = raw.Replace(" банк", "").Replace(" ", "");

        return cleanName;
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
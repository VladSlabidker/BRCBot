using System.Text.RegularExpressions;
using Common.Enums;
using OcrService.Interfaces;
using OcrService.Models;
using Tesseract;

namespace OcrService.Services;

public class TesseractService: IOcrService
{
    private static readonly Dictionary<string, BankType> _bankNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "приват", BankType.Privat24 },
        { "універсал", BankType.Mono },
        { "власнийрахунок", BankType.Bvr },
        { "восток", BankType.Vostok },
        { "мтб", BankType.Mtb },
        { "mtb", BankType.Mtb },
        { "mono", BankType.Mono }
    };
    
    private readonly TesseractEngine _tesseractEngine;

    public TesseractService(TesseractEngine tesseractEngine)
    {
        _tesseractEngine = tesseractEngine;
    }
    
    public Receipt GetReceiptFromImage(string base64)
    {
        // Очистка base64, если с префиксом
        if (base64.Contains(",")) base64 = base64.Split(',')[1];

        var imageBytes = Convert.FromBase64String(base64);
        using var pix = Pix.LoadFromMemory(imageBytes);
        using var page = _tesseractEngine.Process(pix);
        var text = page.GetText();

        const string pattern = @"\b(?:Приватбанк|Універсал\s*банк|Банк\s*Власний\s*Рахунок|Банк\s*Восток|МТБ\s*Банк|MTB\s*Bank|monobank)\b";
        var bankName = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        
        if (bankName.Success)
        {
            string name = NormalizeBankName(bankName.Value);

            if (_bankNameMap.TryGetValue(name, out var bankType))
                return ParseReceiptFromText(bankType);
            else 
                throw new InvalidDataException($"{name} is not a valid bank name");
        }
        else 
            throw new InvalidDataException($"Wrong image with length: {text.Length}");
    }

    private static string NormalizeBankName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        // Приводим к нижнему регистру и убираем лишние пробелы
        raw = raw.ToLowerInvariant();
        raw = Regex.Replace(raw, @"\s+", " ").Trim();

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
    
    private Receipt ParseReceiptFromText(BankType bankType) => 
        bankType switch
        {
            BankType.Mono => new Receipt(),
            BankType.Privat24 => new Receipt(),
            BankType.Vostok => new Receipt(),
            BankType.Bvr => new Receipt(),
            BankType.Mtb => new Receipt(),
            _ => throw new InvalidDataException($"{bankType} is not a valid bank name")
        };
}
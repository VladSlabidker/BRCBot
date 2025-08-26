using System.Globalization;
using System.Text.RegularExpressions;
using Common.Enums;
using OcrService.Models;

namespace OcrService.Services;

public static class BankReceiptParserService
{
    public static Receipt ParseMonoReceipt(BankType bankType, string text)
    {
        Receipt receipt = new Receipt()
        {
            BankId = (int)bankType
        };

        var codeMatch = Regex.Match(text, @"([A-ZА-Я0-9]{4}-[A-ZА-Я0-9]{4}-[A-ZА-Я0-9]{4}-[A-ZА-Я0-9]{4})", RegexOptions.IgnoreCase);
        if (codeMatch.Success)
            receipt.Code = codeMatch.Groups[1].Value.Trim();

        var amountMatch = Regex.Match(text, @"Сума\s*\(грн\)[^\d]*([\d\s]+,\d{2}|\d+.\d{2})");
        if (amountMatch.Success)
        {
            var amountStr = amountMatch.Groups[1].Value
                .Replace(" ", "")
                .Replace(",", ".");

            if (double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                receipt.Amount = amount;
        }

        Console.WriteLine($"Получили чек вида: \nКод: {receipt.Code}, Банк:\n{Enum.GetName(typeof(BankType), bankType)}");
        
        return receipt;
    }
    
    public static Receipt ParsePrivat24Receipt(BankType bankType, string text)
    {
        Receipt receipt = new Receipt()
        {
            BankId = (int)bankType
        };

        var codeMatch = Regex.Match(text, @"Код документа\s+([A-Z0-9]+)");
        if (codeMatch.Success)
            receipt.Code = codeMatch.Groups[1].Value;

        var amountMatch = Regex.Match(text, @"Сума\s+([\d\s]+[.,]\d{2})");
        if (amountMatch.Success)
        {
            var amountStr = amountMatch.Groups[1].Value
                .Replace(" ", "")
                .Replace(",", ".");

            if (double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                receipt.Amount = amount;
        }
        return receipt;
    }    
    
    public static Receipt ParseVostokReceipt(BankType bankType, string text)
    {
        Receipt receipt = new Receipt()
        {
            BankId = (int)bankType
        };

        var codeMatch = Regex.Match(
            text,
            @"\b([A-Z0-9]{4})[-\s]*([A-Z0-9]{4})[-\s]*([A-Z0-9]{4})[-\s]*([A-Z0-9]{4})\b",
            RegexOptions.IgnoreCase
        );

        if (codeMatch.Success)
        {
            receipt.Code = $"{codeMatch.Groups[1].Value}-{codeMatch.Groups[2].Value}-{codeMatch.Groups[3].Value}-{codeMatch.Groups[4].Value}";
        }

        var amountMatch = Regex.Match(text, @"Сума:\s*([\d\s]+[.,]\d{2})");
        if (amountMatch.Success)
        {
            var amountStr = amountMatch.Groups[1].Value
                .Replace(" ", "")
                .Replace(",", ".");

            if (double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                receipt.Amount = amount;
        }

        return receipt;
    }    
    
    public static Receipt ParseMtbReceipt(BankType bankType, string text)
    {
        Receipt receipt = new Receipt()
        {
            BankId = (int)bankType
        };

        var codeMatch = Regex.Match(text, @"Платіжна інструкц[іi]я №\s*([A-Z0-9\-]+)");
        if (codeMatch.Success)
            receipt.Code = codeMatch.Groups[1].Value.Trim();

        var amountMatch = Regex.Match(text, @"Сума\s*[:\-]?\s*([\d\s]+[,\.]\d{2})");
        if (amountMatch.Success)
        {
            var amountStr = amountMatch.Groups[1].Value
                .Replace(" ", "")
                .Replace(",", ".");

            if (double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                receipt.Amount = amount;
        }

        return receipt;
    }
}
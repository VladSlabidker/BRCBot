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
        
        // Пример: "Квитанція № H4XE-8PXM-BBTP-481A від 06.07.2025"
        var codeMatch = Regex.Match(text, @"Квитанц[іi]я Ne\s*([A-Z0-9\-]+)");
        if (codeMatch.Success)
            receipt.Code = codeMatch.Groups[1].Value.Trim();
        
        // Пример: "Сума \(грн\)\s+1 878.00"
        var amountMatch = Regex.Match(text, @"Сума\s*\(грн\)[^\d]*([\d\s]+,\d{2}|\d+.\d{2})");
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
    
    public static Receipt ParsePrivat24Receipt(BankType bankType, string text)
    {
        Receipt receipt = new Receipt()
        {
            BankId = (int)bankType
        };

        //Пример: Код документа P24A4543340891D7379
        var codeMatch = Regex.Match(text, @"Код документа\s+([A-Z0-9]+)");
        if (codeMatch.Success)
            receipt.Code = codeMatch.Groups[1].Value;

        //Пример: Сума 16418,00
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

        //Ппример: Код*: DA21-AC1E-68EE-358D
        var codeMatch = Regex.Match(text, @"Код\*:\s*([A-Z0-9\-]+)");
        if (codeMatch.Success)
            receipt.Code = codeMatch.Groups[1].Value;

        //Пример: Сума: 2 501.00
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
        // Пример: "Платіжна інструкція № 49VE-82IF-72IF-5C0V від 29.06.2025"
        var codeMatch = Regex.Match(text, @"Платіжна інструкц[іi]я №\s*([A-Z0-9\-]+)");
        if (codeMatch.Success)
            receipt.Code = codeMatch.Groups[1].Value.Trim();
        
        // Пример: "Сума: 4 500,00 грн"
        var amountMatch = Regex.Match(text, @"Сума\s*[:\-]?\s*([\d\s]+[,\.]\d{2})");
        if (amountMatch.Success)
        {
            var amountStr = amountMatch.Groups[1].Value
                .Replace(" ", "") // убираем пробелы
                .Replace(",", "."); // заменяем запятую на точку

            if (double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                receipt.Amount = amount;
        }

        return receipt;
    }
}
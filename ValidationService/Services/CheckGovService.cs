using Microsoft.Playwright;
using Common.Enums;
using ValidationService.Interfaces;

namespace ValidationService.Services;

public static class CheckGovService
{
    public static async Task<(bool Success, string Message)> ValidateReceiptAsync(BankType bankType, string code)
    {
        const string baseUrl = "https://check.gov.ua";
        
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        Console.WriteLine("Заходим на сайт");
        await page.GotoAsync(baseUrl);
        
        try
        {
            // Открытие выпадающего списка
            await page.WaitForSelectorAsync(".select-selected");
            await page.ClickAsync(".select-selected");

            // Поиск и выбор нужного банка
            var bankText = GetBankText(bankType);
            await page.WaitForSelectorAsync(".selection-list");
            await page.ClickAsync($".selection-list div:text('{bankText}')");

            // Ввод кода вручную через JS
            await page.EvaluateAsync($@"
                () => {{
                    const input = document.querySelector('#references');
                    input.value = '{code}';
                    input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                }}");

            // Ждём разблокировку кнопки и кликаем по ней
            await Task.Delay(500);
            await page.EvaluateAsync("document.querySelector('#submit').click()");

            // Ожидаем появления блока результата
            await page.WaitForSelectorAsync("#checkResult", new() { Timeout = 7000 });

            var resultText = await page.InnerTextAsync("#resultFlag");

            Console.WriteLine($"Результат со страницы \n{resultText}");

            if (resultText.Contains("Помилка", StringComparison.OrdinalIgnoreCase))
                return (false, "Сайт повернув помилку: код може бути недійсним або банк не підтримується");

            if (resultText.Contains("Оплачена", StringComparison.OrdinalIgnoreCase))
            {
                // Пробуем достать ссылку
                var downloadHref = await page.GetAttributeAsync("#resultFile", "href");

                if (!string.IsNullOrWhiteSpace(downloadHref))
                {
                    var fullLink = downloadHref.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? downloadHref
                        : $"{baseUrl}{downloadHref}";

                    return (true, fullLink);
                }

                return (true, string.Empty);
            }

            return (false, $"Невідомий результат: {resultText}");
        }
        catch (TimeoutException)
        {
            return (false, "Не вдалося отримати результат: таймаут або помилка сайту.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    
    private static string GetBankText(BankType type) =>
        type switch
        {
            BankType.Mono => "Монобанк",
            BankType.Mtb => "МТБ БАНК",
            BankType.Vostok => "Банк Власний Рахунок | Восток",
            BankType.Bvr => "Банк Власний Рахунок | Восток",
            _ => throw new ArgumentOutOfRangeException($"Unknown bank: {type}")
        };

}

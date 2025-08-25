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
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true, Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" } });
        var page = await browser.NewPageAsync();
        Console.WriteLine("Заходим на сайт");
        await page.GotoAsync(baseUrl, new()
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000
        });
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        try
        {
            Console.WriteLine($"Открываем страницу.");
            // Открытие выпадающего списка
            Console.WriteLine(await page.ContentAsync());
            await page.WaitForSelectorAsync(".select-selected", new()
            {
                Timeout = 60000,
                State = WaitForSelectorState.Visible
            });
            await page.ClickAsync(".select-selected", new()
            {
                Timeout = 30000,
                Force = true // иногда нужно
            });
            Console.WriteLine($"Кликнули на селектор.");
            // Поиск и выбор нужного банка
            var bankText = GetBankText(bankType);
            Console.WriteLine($"Банк: {bankText}.");
            await page.WaitForSelectorAsync(".selection-list");
            Console.WriteLine($"Выбираем банк");
            await page.ClickAsync($".selection-list div:text('{bankText}')");
            Console.WriteLine($"Выбрали банк");
            // Ввод кода вручную через JS
            Console.WriteLine($"Вставляем код:{code}");
            await page.EvaluateAsync($@"
                () => {{
                    const input = document.querySelector('#references');
                    input.value = '{code}';
                    input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                }}");

            // Ждём разблокировку кнопки и кликаем по ней
            Console.WriteLine($"Ждем разблокировку");
            await Task.Delay(500);
            Console.WriteLine($"Дождались разблокировку");
            Console.WriteLine($"Проверяем");
            await page.EvaluateAsync("document.querySelector('#submit').click()");

            // Ожидаем появления блока результата
            Console.WriteLine($"Ожидаем результат");
            await page.WaitForSelectorAsync("#checkResult", new() { Timeout = 30000 });
            Console.WriteLine($"Получили результат");
            var resultText = await page.InnerTextAsync("#resultFlag");

            Console.WriteLine($"Результат со страницы \n{resultText}");

            if (resultText.Contains("Помилка", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(resultText);

            if (resultText.Contains("Оплачена", StringComparison.OrdinalIgnoreCase))
            {
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

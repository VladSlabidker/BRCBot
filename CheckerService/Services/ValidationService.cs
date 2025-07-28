using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using Common.Enums;

namespace CheckerService.Services;

public class ValidationService
{
    private static string GetBankText(BankType type) =>
        type switch
        {
            BankType.Privat24 => "Приватбанк",
            BankType.Mono => "Монобанк",
            BankType.Mtb => "МТБ БАНК",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Неизвестный банк: {type}")
        };

public async Task<(bool Success, string Message)> ValidateReceiptAsync(BankType bankType, string code)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync("https://check.gov.ua");

        // Открытие дропдауна
        await page.WaitForSelectorAsync(".select-selected");
        await page.ClickAsync(".select-selected");

        // Ждём появления списка
        await page.WaitForSelectorAsync(".selection-list");

        // Клик по нужному банку
        var bankText = GetBankText(bankType);
        await page.ClickAsync($".selection-list div:text('{bankText}')");

        // Вводим код вручную через JS и триггерим событие input
        await page.EvaluateAsync($@"
            () => {{
                const input = document.querySelector('#references');
                input.value = '{code}';
                input.dispatchEvent(new Event('input', {{ bubbles: true }}));
            }}");

        // Ожидаем, пока кнопка станет активной, затем кликаем по-нормальному
        await Task.Delay(500);
        await page.EvaluateAsync("document.querySelector('#submit').click()");

        // Ожидание блока результата
        try
        {
            await page.WaitForSelectorAsync("#checkResult", new() { Timeout = 7000 });

            string resultText = await page.InnerTextAsync("#resultFlag");

            // Если ошибка — сохраняем HTML и скриншот
            if (resultText.Contains("Помилка", StringComparison.OrdinalIgnoreCase))
                return (false, "Сайт вернул ошибку: Помилка — возможно, код недействителен или банк не поддерживается");


            if (resultText.Contains("Оплачена", StringComparison.OrdinalIgnoreCase))
                return (true, page.ContentAsync().Result);

            return (false, $"Неизвестный результат: {resultText}");
        }
        catch (TimeoutException)
        {
            return (false, "Не удалось получить результат: таймаут");
        }
    }
}
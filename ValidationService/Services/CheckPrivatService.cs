using Microsoft.Playwright;
using Common.Enums;
using ValidationService.Interfaces;

namespace ValidationService.Services;

public static class CheckPrivatService
{
    public static async Task<(bool Success, string Message)> ValidateReceiptAsync(BankType bankType, string code)
    {
        if (bankType != BankType.Privat24)
            return (false, $"CheckPrivatService підтримує тільки ПриватБанк");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        const string baseUrl = "https://privatbank.ua";
        await page.GotoAsync($"{baseUrl}/check");

        await page.WaitForSelectorAsync("#docNumber");

        await page.EvaluateAsync($@"
            () => {{
                const input = document.querySelector('#docNumber');
                input.value = '{code}';
                input.dispatchEvent(new Event('input', {{ bubbles: true }}));
            }}");
        
        await page.ClickAsync("button.validate-btn");

        try
        {
            await page.WaitForSelectorAsync(".after_send_block", new() { Timeout = 7000 });
            
            var href = await page.GetAttributeAsync("#download_link", "href");

            if (!string.IsNullOrEmpty(href))
            {
                var fullLink = href.StartsWith("https", StringComparison.OrdinalIgnoreCase)
                    ? href
                    : $"{baseUrl}{href}";

                return (true, fullLink);
            }

            var fileName = await page.InnerTextAsync("#file_name_paragraph");
            return (true, string.Empty);
        }
        catch (TimeoutException)
        {
            return (false, "Не вдалося отримати результат: таймаут або документ не знайдено.");
        }
    }
}

using Telegram.Bot;
using Telegram.Gateway.Interfaces;
using Telegram.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);
    # if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.Telegram.Gateway.json");
    # endif
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("/app/appsettings.Telegram.Gateway.json");
string token = builder.Configuration["Telegram:BotToken"] 
               ?? throw new InvalidOperationException("Telegram token not configured");

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

// Другие сервисы
builder.Services.AddHttpClient("Storefront", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Storefront:BaseUrl"]! ?? throw new InvalidOperationException("Storefront URL not configured"));
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });

builder.Services.AddScoped<ITelegramUpdateService, TelegramUpdateService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
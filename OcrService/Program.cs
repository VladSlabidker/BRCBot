
using Microsoft.Extensions.Options;
using OcrService.Configs;
using Tesseract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Configuration.AddJsonFile("appsettings.Development.json");

builder.Services.Configure<OcrConfig>(builder.Configuration.GetSection(nameof(OcrConfig)));

builder.Services.AddScoped<TesseractEngine>(provider =>
{
    var config = provider.GetRequiredService<IOptions<OcrConfig>>().Value;
    return new TesseractEngine(config.TessDataPath, config.Languages, EngineMode.Default);
});

var app = builder.Build();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
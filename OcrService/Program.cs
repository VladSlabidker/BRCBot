using Common.Interceptors;
using Microsoft.Extensions.Options;
using OcrService.Configs;
using OcrService.Extensions;
using OcrService.Profiles;
using OcrService.Services;
using Tesseract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionHandlingInterceptor>();
});

# if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.OcrService.json");
# endif
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<OcrConfig>(builder.Configuration.GetSection(nameof(OcrConfig)));
builder.Services.Configure<RabbitMqConfig>(builder.Configuration.GetSection(nameof(RabbitMqConfig)));

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddScoped<TesseractEngine>(provider =>
{
    var config = provider.GetRequiredService<IOptions<OcrConfig>>().Value;
    return new TesseractEngine(config.TessDataPath, config.Languages, EngineMode.Default);
});

builder.Services.AddScoped<TesseractService>();
builder.Services.AddTransient<PaddleService>();
builder.Services.AddAutoMapper(typeof(OcrProfile));

var app = builder.Build();

app.MapGrpcService<OcrService.Services.gRPC.RpcOcrService>();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
using Data.Cache.Extensions;
using Data.SQL;
using Data.SQL.Configs;
using ValidationService.Configs;
using ValidationService.Services;
using Common.Enums;
using Common.Interceptors;

var res = await CheckGovService.ValidateReceiptAsync(BankType.Mono, "H4ХЕ-8РХМ-ВВТР-481А");
Console.WriteLine(res);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Development.json");
UriConfig configurationUri = new UriConfig();
builder.Configuration.GetSection(nameof(UriConfig)).Bind(configurationUri);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionHandlingInterceptor>();
});

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddDbContext<BotContext>();
builder.Services.Configure<SqlExpressConfig>(builder.Configuration.GetSection(nameof(SqlExpressConfig)));
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddGrpcClient<RpcOcrService.RpcOcrService.RpcOcrServiceClient>(opt => opt.Address = configurationUri.OcrService).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});
builder.Services.AddScoped<ValidationService.Services.ValidationService>();

var app = builder.Build();

app.MapGrpcService<ValidationService.Services.gRPC.RpcValidationService>();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
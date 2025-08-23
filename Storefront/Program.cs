using Common.Interceptors;
using Storefront.Configs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

# if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.Storefront.json");
# endif
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("/app/appsettings.Storefront.json");
UriConfig configurationUri = new UriConfig();
builder.Configuration.GetSection(nameof(UriConfig)).Bind(configurationUri);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionHandlingInterceptor>();
});

builder.Services.AddGrpcClient<RpcOcrService.RpcOcrService.RpcOcrServiceClient>(opt => opt.Address = configurationUri.OcrService).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});
builder.Services.AddGrpcClient<RpcValidationService.RpcValidationService.RpcValidationServiceClient>(opt => opt.Address = configurationUri.ValidationService).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();

using Data.Cache.Extensions;

var builder = WebApplication.CreateBuilder(args);

# if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.Data.Cache.json");
# endif
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("/app/appsettings.Data.Cache.json");
builder.Services.AddRedisCache(builder.Configuration);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
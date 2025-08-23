using Data.Cache.Extensions;

var builder = WebApplication.CreateBuilder(args);

# if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.json");
# endif
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRedisCache(builder.Configuration);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
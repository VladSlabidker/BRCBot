using Data.SQL;
using Data.SQL.Configs;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Development.json");

builder.Services.AddGrpc();
builder.Services.AddDbContext<BotContext>();
builder.Services.Configure<SqlExpressConfig>(builder.Configuration.GetSection(nameof(SqlExpressConfig)));

var app = builder.Build();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
using Data.SQL;
using Data.SQL.Configs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

# if DEBUG
builder.Configuration.AddJsonFile("appsettings.Development.Data.SQL.json");
# endif
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddGrpc();
builder.Services.AddDbContext<BotContext>();
builder.Services.Configure<SqlExpressConfig>(builder.Configuration.GetSection(nameof(SqlExpressConfig)));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BotContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
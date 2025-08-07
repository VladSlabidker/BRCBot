using Data.Cache.Extensions;
using Data.SQL;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using SubscriptionService.Configs;
using SubscriptionService.Interfaces;
using SubscriptionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Development.json");

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<BillingOptions>(
    builder.Configuration.GetSection("Billing"));

builder.Services.AddGrpc();
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddDbContext<BotContext>();
builder.Services.AddSingleton<MockBillingProvider>();
builder.Services.AddTransient<IBillingProvider>(sp =>
{
    var config = sp.GetRequiredService<IOptions<BillingOptions>>().Value;
    return config.Provider switch
    {
        "Mock" => sp.GetRequiredService<MockBillingProvider>(),
        _ => throw new NotSupportedException("Unknown billing provider")
    };
});

builder.Services.AddScoped<ISubscriptionService, SubscriptionService.Services.SubscriptionService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SubscriptionBillingConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var opts = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        cfg.Host(opts.Host, h =>
        {
            h.Username(opts.Username);
            h.Password(opts.Password);
        });

        cfg.ReceiveEndpoint(opts.Queue, e =>
        {
            e.ConfigureConsumer<SubscriptionBillingConsumer>(context);
        });
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5005, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

app.MapGrpcService<SubscriptionService.Services.gRPC.RpcSubscriptionService>();

app.Run();
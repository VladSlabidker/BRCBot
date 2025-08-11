using MassTransit;
using OcrService.Configs;
using OcrService.Models;

namespace OcrService.Extensions ;

public static class MassTransitExtension
{
    public static void AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConfig = configuration.GetSection(nameof(RabbitMqConfig)).Get<RabbitMqConfig>();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfig.HostName, (ushort)rabbitMqConfig.Port, "/", h =>
                {
                    h.Username(rabbitMqConfig.UserName);
                    h.Password(rabbitMqConfig.Password);
                });
            });

            x.AddRequestClient<OcrRequest>(new Uri($"queue:{rabbitMqConfig.RequestQueueName}"));
        });
    }
}
using Microsoft.Extensions.Options;
using OcrService.Configs;
using OcrService.Interfaces;
using OcrService.Services;

namespace OcrService.Extensions
{
    public static class RabbitMqExtension
    {
        public static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqConfig = configuration
                .GetSection(nameof(RabbitMqConfig))
                .Get<RabbitMqConfig>() ?? throw new InvalidOperationException("RabbitMqConfig section is missing");

            if (string.IsNullOrWhiteSpace(rabbitMqConfig.HostName))
                throw new InvalidOperationException("RabbitMqConfig.HostName is not set");

            if (string.IsNullOrWhiteSpace(rabbitMqConfig.RequestQueueName))
                throw new InvalidOperationException("RabbitMqConfig.RequestQueueName is not set");

            if (string.IsNullOrWhiteSpace(rabbitMqConfig.ResponseQueueName))
                throw new InvalidOperationException("RabbitMqConfig.ResponseQueueName is not set");

            services.Configure<RabbitMqConfig>(configuration.GetSection(nameof(RabbitMqConfig)));

            services.AddSingleton<IRabbitMqClient>(sp =>
            {
                var cfg = sp.GetRequiredService<IOptions<RabbitMqConfig>>().Value;

                var client = new RabbitMqClient(cfg);

                client.InitializeAsync().GetAwaiter().GetResult();

                return client;
            });
        }
    }
}
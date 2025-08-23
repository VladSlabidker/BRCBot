using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OcrService.Configs;
using OcrService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public sealed class RabbitMqClient : IRabbitMqClient, IAsyncDisposable
{
    private readonly RabbitMqConfig _config;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<OcrResponse>> _pendingRequests = new();

    public RabbitMqClient(RabbitMqConfig options)
    {
        _config = options;
    }

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        // Очередь запросов
        await _channel.ExchangeDeclareAsync($"{_config.RequestQueueName}_exchange", ExchangeType.Direct, durable: true);
        await _channel.QueueDeclareAsync(_config.RequestQueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(_config.RequestQueueName, $"{_config.RequestQueueName}_exchange", _config.RequestQueueName);

        // Очередь ответов
        await _channel.ExchangeDeclareAsync($"{_config.ResponseQueueName}_exchange", ExchangeType.Direct, durable: true);
        await _channel.QueueDeclareAsync(_config.ResponseQueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(_config.ResponseQueueName, $"{_config.ResponseQueueName}_exchange", _config.ResponseQueueName);

        // Консюмер для ответов
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray(); // ← исправлено
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<OcrResponse>(json, _jsonOptions);

            if (message != null && _pendingRequests.TryRemove(message.CorrelationId, out var tcs))
            {
                tcs.TrySetResult(message);
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await _channel.BasicConsumeAsync(_config.ResponseQueueName, autoAck: false, consumer: consumer);
    }

    public async Task<OcrResponse> SendRequestAndWaitForResponseAsync(OcrRequest request, CancellationToken cancellationToken = default)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMqClient is not initialized");

        request.CorrelationId = Guid.NewGuid();

        var tcs = new TaskCompletionSource<OcrResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[request.CorrelationId] = tcs;

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request, _jsonOptions));

        // Новый способ создания свойств
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            CorrelationId = request.CorrelationId.ToString(),
            ReplyTo = _config.ResponseQueueName
        };

        // Generic метод, явно указываем тип
        await _channel.BasicPublishAsync(
            exchange: $"{_config.RequestQueueName}_exchange",
            routingKey: _config.RequestQueueName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken
        );

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
    
        linkedCts.Token.Register(() => tcs.TrySetCanceled());

        try
        {
            return await tcs.Task;
        }
        catch (TaskCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException("RabbitMQ timeout exceeded");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
    }
}

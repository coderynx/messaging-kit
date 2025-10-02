using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Coderynx.MessagingKit.Transports.RabbitMq;

internal sealed class RabbitMqMessageBus(
    ILogger<RabbitMqMessageBus> logger,
    MessageDispatcher dispatcher,
    MessageBusOptionsBase options) : IMessageBus
{
    private const string ExchangeName = "exchange";
    private readonly RabbitMqOptions _options = (RabbitMqOptions)options;
    private IChannel _channel = null!;
    private IConnection? _connection;

    public bool IsInitialized => _connection?.IsOpen ?? false;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = _options.Uri,
            ClientProvidedName = _options.ClientName
        };

        _connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        logger.LogInformation("Initialized RabbitMQ bus {BusName}", options.BusName);

        var queueNames = options.MessageRegistrations
            .Where(registration => registration.ConsumerType is not null)
            .Select(registration => registration.GetTopicName());

        foreach (var queueName in queueNames)
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: queueName,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += OnConsumerOnReceivedAsync;

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            logger.LogInformation("Listening for messages on queue {QueueName}", queueName);
        }

        logger.LogInformation("Listening for messages on exchange {ExchangeName}", ExchangeName);
    }

    public async Task PublishAsync(Letter letter, CancellationToken cancellationToken = default)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized yet.");
        }

        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: letter.GetTopicName(),
            mandatory: false,
            basicProperties: letter.ToBasicProperties(),
            body: letter.Payload,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        logger.LogInformation("Terminating RabbitMQ bus {BusName}", options.BusName);

        await _channel.DisposeAsync();

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        logger.LogInformation("Terminated RabbitMQ bus {BusName}", options.BusName);
    }

    private async Task OnConsumerOnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var letter = ea.BasicProperties.ToLetter(ea.Body);

        await dispatcher.DispatchAsync(letter, ea.CancellationToken);
        await _channel.BasicAckAsync(ea.DeliveryTag, false, ea.CancellationToken);
    }
}
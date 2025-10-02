using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.Logging;

namespace Coderynx.MessagingKit;

public sealed class MessagePublisher(MessageBusManager busManager, ILogger<MessagePublisher> logger) : IMessagePublisher
{
    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var bus = busManager.ResolveBus<TMessage>();
        if (bus is null)
        {
            logger.LogWarning("No bus found for message type {MessageType}", typeof(TMessage));
            return;
        }

        var letter = Letter.Create([], PayloadFormat.Json, message);

        logger.LogInformation("Publishing message to bus {BusName}", bus.Value.Name);

        try
        {
            await bus.Value.MessageBus.PublishAsync(letter, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new MessagePublishingException(
                busName: bus.Value.Name,
                message: $"Failed to publish message to bus {bus.Value.Name}",
                innerException: exception);
        }

        logger.LogInformation("Message published to bus {BusName}", bus.Value.Name);
    }
}
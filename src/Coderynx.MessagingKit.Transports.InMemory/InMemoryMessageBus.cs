using System.Threading.Channels;
using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.Logging;

namespace Coderynx.MessagingKit.Transports.InMemory;

internal sealed class InMemoryMessageBus(
    ILogger<InMemoryMessageBus> logger,
    MessageDispatcher dispatcher,
    MessageBusOptionsBase options) : IMessageBus
{
    private readonly Dictionary<string, Channel<Letter>> _channels = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _cts = new();
    private readonly Dictionary<Type, string> _typeToTopic = new();
    private readonly List<Task> _workers = [];

    public bool IsInitialized => !_cts.IsCancellationRequested;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Initialized InMemory bus {BusName}", options.BusName);

        var registrations = options.MessageRegistrations
            .Where(registration => registration.ConsumerType is not null)
            .ToArray();

        foreach (var registration in registrations)
        {
            var topic = registration.GetTopicName();
            _typeToTopic[registration.MessageType] = topic;

            var channelOptions = new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = true
            };

            var channel = Channel.CreateUnbounded<Letter>(channelOptions);

            _channels[topic] = channel;

            var worker = Task.Run(() => WorkerAsync(topic, channel.Reader, _cts.Token), cancellationToken);
            _workers.Add(worker);

            logger.LogInformation("Listening for messages on topic {Topic}", topic);
        }

        return Task.CompletedTask;
    }

    public async Task PublishAsync(Letter letter, CancellationToken cancellationToken = default)
    {
        if (!_typeToTopic.TryGetValue(letter.PayloadType, out var topic))
        {
            topic = string.Concat(
                letter.PayloadType.Name.Select((x, i) => i > 0 && char.IsUpper(x)
                    ? "-" + char.ToLower(x)
                    : char.ToLower(x).ToString()));
        }

        logger.LogTrace("InMemory publish to topic {Topic}", topic);

        if (!_channels.TryGetValue(topic, out var channel))
        {
            logger.LogWarning(
                "No in-memory consumer channel found for topic {Topic}. Message will not be delivered.",
                topic);

            return;
        }

        await channel.Writer.WriteAsync(letter, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        logger.LogInformation("Terminating InMemory bus {BusName}", options.BusName);

        try
        {
            await _cts.CancelAsync();
        }
        catch
        {
            // ignore
        }

        foreach (var channel in _channels.Values)
        {
            try
            {
                channel.Writer.TryComplete();
            }
            catch
            {
                // ignore
            }
        }

        try
        {
            await Task.WhenAll(_workers);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error while awaiting in-memory workers shutdown");
        }

        _cts.Dispose();

        logger.LogInformation("Terminated InMemory bus {BusName}", options.BusName);
    }

    private async Task WorkerAsync(string topic, ChannelReader<Letter> reader, CancellationToken cancellationToken)
    {
        try
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                while (reader.TryRead(out var letter))
                {
                    try
                    {
                        await dispatcher.DispatchAsync(letter, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to dispatch in-memory message on topic {Topic}", topic);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in in-memory worker for topic {Topic}", topic);
        }
    }
}
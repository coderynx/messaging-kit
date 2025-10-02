namespace Coderynx.MessagingKit.Abstractions;

/// <summary>
///     Defines an abstraction for a message bus that enables asynchronous communication
///     through the publishing and handling of messages.
/// </summary>
public interface IMessageBus : IAsyncDisposable
{
    /// <summary>
    ///     Asynchronously initializes the message bus, setting up necessary connections, channels,
    ///     exchanges, and queues for message handling.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests during the initialization process.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous initialization operation.
    /// </returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously publishes a message, encapsulated in a <see cref="Letter" /> object,
    ///     to the configured message bus.
    /// </summary>
    /// <param name="letter">
    ///     The letter object representing the message to be published, including its payload,
    ///     headers, and other metadata.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests while publishing the message.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous publish operation.
    /// </returns>
    Task PublishAsync(Letter letter, CancellationToken cancellationToken = default);
}
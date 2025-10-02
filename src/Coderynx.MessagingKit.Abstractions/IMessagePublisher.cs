namespace Coderynx.MessagingKit.Abstractions;

/// <summary>
///     Defines an interface for publishing messages asynchronously to a message bus.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    ///     Publishes a message asynchronously to a message bus.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message being published. Must be a reference type.</typeparam>
    /// <param name="message">The message to be published.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class;
}
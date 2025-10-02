namespace Coderynx.MessagingKit.Abstractions;

/// <summary>
///     Represents a marker interface for consumers.
/// </summary>
public interface IConsumer;

/// <summary>
///     Defines a generic consumer interface for processing messages of a specific type
///     within a messaging infrastructure.
/// </summary>
/// <typeparam name="TMessage">The type of message to be processed by the consumer.</typeparam>
public interface IConsumer<TMessage> : IConsumer where TMessage : class
{
    /// <summary>
    ///     Asynchronously consumes and processes a message wrapped in a <see cref="ConsumerContext{TMessage}" />.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message being consumed.</typeparam>
    /// <param name="context">
    ///     The <see cref="ConsumerContext{TMessage}" /> containing the message and associated metadata.
    /// </param>
    /// <param name="ct">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConsumeAsync(ConsumerContext<TMessage> context, CancellationToken ct = default);
}
namespace Coderynx.MessagingKit.Abstractions;

public sealed record ConsumerContext<TMessage> where TMessage : class
{
    internal ConsumerContext(TMessage message, IReadOnlyDictionary<string, string?> headers)
    {
        Message = message;
        Headers = headers;
    }

    public IReadOnlyDictionary<string, string?> Headers { get; }
    public TMessage Message { get; }
}
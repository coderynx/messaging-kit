using Coderynx.MessagingKit.Abstractions;

namespace Coderynx.MessagingKit.Transports.RabbitMq;

public sealed record RabbitMqOptions : MessageBusOptionsBase
{
    public RabbitMqOptions(
        string busName,
        Uri uri,
        string clientName,
        HashSet<MessageRegistration> messageRegistrations) : base(typeof(RabbitMqMessageBus), busName,
        messageRegistrations)
    {
        Uri = uri;
        ClientName = clientName;
    }

    public Uri Uri { get; init; }
    public string? ClientName { get; init; }
}
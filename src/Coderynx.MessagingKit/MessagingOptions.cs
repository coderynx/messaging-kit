using Coderynx.MessagingKit.Abstractions;

namespace Coderynx.MessagingKit;

public sealed record MessagingOptions
{
    public List<MessageBusOptionsBase> BusOptions { get; init; } = [];
}
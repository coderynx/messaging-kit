using Coderynx.MessagingKit.Abstractions;

namespace Coderynx.MessagingKit.Transports.InMemory;

public sealed record InMemoryOptions(
    string BusName,
    HashSet<MessageRegistration> MessageRegistrations)
    : MessageBusOptionsBase(typeof(InMemoryMessageBus), BusName, MessageRegistrations);
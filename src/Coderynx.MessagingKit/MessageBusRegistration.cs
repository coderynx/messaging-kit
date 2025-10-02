using Coderynx.MessagingKit.Abstractions;

namespace Coderynx.MessagingKit;

public sealed record MessageBusRegistration(MessageBusOptionsBase Options, IMessageBus Bus);
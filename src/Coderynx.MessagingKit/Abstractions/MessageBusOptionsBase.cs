namespace Coderynx.MessagingKit.Abstractions;

public abstract record MessageBusOptionsBase(
    Type MessageBusType,
    string BusName,
    HashSet<MessageRegistration> MessageRegistrations);
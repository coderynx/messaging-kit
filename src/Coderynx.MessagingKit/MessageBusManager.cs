using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Coderynx.MessagingKit;

public sealed class MessageBusManager(IOptions<MessagingOptions> options, IServiceProvider serviceProvider)
{
    private readonly List<MessageBusRegistration> _registrations = [];

    public void InitializeBuses()
    {
        foreach (var busOptions in options.Value.BusOptions)
        {
            var bus = (IMessageBus)ActivatorUtilities.CreateInstance(
                provider: serviceProvider,
                instanceType: busOptions.MessageBusType,
                parameters: busOptions);

            var registration = new MessageBusRegistration(busOptions, bus);
            _registrations.Add(registration);
        }

        foreach (var registration in _registrations)
        {
            _ = Task.Factory.StartNew(
                async () => await registration.Bus.InitializeAsync(),
                TaskCreationOptions.LongRunning);
        }
    }

    public IMessageBus? ResolveBus(string busName)
    {
        var registration = _registrations.FirstOrDefault(r => r.Options.BusName.Equals(busName));
        return registration?.Bus;
    }

    public (string Name, IMessageBus MessageBus)? ResolveBus<TMessage>() where TMessage : class
    {
        var registration = _registrations.FirstOrDefault(r =>
            r.Options.MessageRegistrations.Any(h => h.MessageType == typeof(TMessage)));

        if (registration is null)
        {
            return null;
        }

        return (registration.Options.BusName, registration.Bus);
    }
}
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

        foreach (var bus in _registrations.Select(registration => registration.Bus))
        {
            _ = Task.Factory.StartNew(
                async () => await bus.InitializeAsync(),
                TaskCreationOptions.LongRunning);
        }
    }

    public void WaitForBusesInitialization()
    {
        var buses = _registrations
            .Select(registration => registration.Bus)
            .ToList();

        if (buses.Count is 0)
        {
            return;
        }

        var resetEvent = new ManualResetEventSlim(false);
        var timer = new Timer(CheckIfBusesAreInitialized, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(50));

        try
        {
            resetEvent.Wait();
        }
        finally
        {
            timer.Dispose();
            resetEvent.Dispose();
        }

        return;

        void CheckIfBusesAreInitialized(object? _)
        {
            if (buses.All(bus => bus.IsInitialized))
            {
                // ReSharper disable once AccessToDisposedClosure
                resetEvent.Set();
            }
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
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit.Abstractions;

public abstract class BusBuilderBase(IServiceCollection services)
{
    protected HashSet<MessageRegistration> MessageRegistrations { get; } = new();

    public void WithEvent<TEvent>(string? topic = null) where TEvent : class
    {
        var eventType = typeof(TEvent);
        var consumerInterface = typeof(IConsumer<>).MakeGenericType(eventType);

        var consumerType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t is { IsClass: true, IsAbstract: false } && consumerInterface.IsAssignableFrom(t));

        var messageRegistration = new MessageRegistration(eventType, consumerType, topic);
        MessageRegistrations.Add(messageRegistration);

        if (consumerType is not null)
        {
            services.AddScoped(consumerInterface, consumerType);
        }
    }
}
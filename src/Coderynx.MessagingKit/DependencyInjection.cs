using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coderynx.MessagingKit;

public static class DependencyInjection
{
    public static void AddMessaging(this IServiceCollection services, Action<MessagingBuilder> configure)
    {
        var messagingBuilder = new MessagingBuilder(services);
        configure.Invoke(messagingBuilder);

        services.AddSingleton<MessageBusManager>();
        services.AddSingleton<MessageDispatcher>();

        services.AddScoped<IMessagePublisher, MessagePublisher>();
    }
}
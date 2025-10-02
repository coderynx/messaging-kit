using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coderynx.MessagingKit;

public static class DependencyInjection
{
    public static void AddMessaging(this IHostApplicationBuilder builder, Action<MessagingBuilder> configure)
    {
        var messagingBuilder = new MessagingBuilder(builder.Services);
        configure.Invoke(messagingBuilder);

        builder.Services.AddSingleton<MessageBusManager>();
        builder.Services.AddSingleton<MessageDispatcher>();

        builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();
    }
}
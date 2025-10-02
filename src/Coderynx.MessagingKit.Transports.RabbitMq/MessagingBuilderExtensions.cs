using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit.Transports.RabbitMq;

public static class MessagingBuilderExtensions
{
    public static void AddRabbitMq(this MessagingBuilder builder, Action<RabbitMqBuilder> configure)
    {
        var rabbitMqBuilder = new RabbitMqBuilder(builder.Services);
        configure(rabbitMqBuilder);

        var options = rabbitMqBuilder.Build();
        builder.Services.Configure<MessagingOptions>(messagingOptions => messagingOptions.BusOptions.Add(options));

        builder.Services.AddSingleton<IMessageBus>(sp =>
        {
            var busProvider = sp.GetRequiredService<MessageBusManager>();

            var bus = busProvider.ResolveBus(options.BusName);
            if (bus is null)
            {
                throw new ApplicationException($"Message bus '{options.BusName}' not found");
            }

            return bus;
        });
    }
}
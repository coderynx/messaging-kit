using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit.Transports.InMemory;

public static class MessagingBuilderExtensions
{
    public static void AddInMemory(this MessagingBuilder builder, Action<InMemoryBuilder> configure)
    {
        var inMemoryBuilder = new InMemoryBuilder(builder.Services);
        configure(inMemoryBuilder);

        var options = inMemoryBuilder.Build();
        builder.Services.Configure<MessagingOptions>(messagingOptions => messagingOptions.BusOptions.Add(options));

        builder.Services.AddSingleton<IMessageBus>(sp =>
        {
            var busProvider = sp.GetRequiredService<MessageBusManager>();

            return busProvider.ResolveBus(options.BusName) ??
                   throw new ApplicationException($"Message bus '{options.BusName}' not found");
        });
    }
}
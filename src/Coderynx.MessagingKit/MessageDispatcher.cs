using System.Reflection;
using System.Text.Json;
using Coderynx.MessagingKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit;

public sealed class MessageDispatcher(IServiceScopeFactory serviceScopeFactory)
{
    public async Task DispatchAsync(Letter letter, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize(letter.Payload, letter.PayloadType) ??
                      throw new InvalidOperationException("Failed to deserialize message.");

        var consumerType = typeof(IConsumer<>).MakeGenericType(letter.PayloadType);

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService(consumerType);

        var contextType = typeof(ConsumerContext<>).MakeGenericType(letter.PayloadType);

        var context = Activator.CreateInstance(
            type: contextType,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [message, letter.Headers],
            culture: null);

        await ((dynamic)consumer).ConsumeAsync((dynamic?)context, cancellationToken);
    }
}
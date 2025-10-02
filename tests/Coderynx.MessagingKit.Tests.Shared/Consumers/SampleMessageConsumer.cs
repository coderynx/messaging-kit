using Coderynx.MessagingKit.Abstractions;
using Coderynx.MessagingKit.Tests.Shared.TestSupport;

namespace Coderynx.MessagingKit.Tests.Shared.Consumers;

public sealed record SampleMessage(string Text);

public sealed class SampleMessageConsumer(TestProbe probe) : IConsumer<SampleMessage>
{
    public Task ConsumeAsync(ConsumerContext<SampleMessage> context, CancellationToken ct = default)
    {
        probe.Record(context.Message);
        return Task.CompletedTask;
    }
}
using Coderynx.MessagingKit.Tests.Shared.Consumers;
using Coderynx.MessagingKit.Tests.Shared.TestSupport;
using Shouldly;

namespace Coderynx.MessagingKit.Transports.InMemory.IntegrationTests;

public sealed class BusAvailableTests(ApplicationFactory applicationFactory) : IntegrationTestsBase(applicationFactory)
{
    [Fact]
    public async Task ConsumeAsync_ShouldConsumeMessage()
    {
        // Arrange
        var message = new SampleMessage("hello in-memory");
        await Publisher.PublishAsync(message);

        // Act
        var received = await WaitHelper.WaitUntilAsync(
            () => Probe.Messages.OfType<SampleMessage>().Any(m => m.Text.Equals("hello in-memory")),
            timeout: TimeSpan.FromSeconds(5));

        // Assert
        received.ShouldBeTrue("Message was not received within the timeout period.");
    }

    [Fact]
    public async Task PublishAsync_ShouldPublishMessage()
    {
        // Arrange
        var message = new SampleMessage("hello in-memory");

        // Act & Assert
        await Should.NotThrowAsync(() => Publisher.PublishAsync(message));
    }

    [Fact]
    public async Task ConsumeAsync_ShouldTimeoutWhenNoMessage()
    {
        // Arrange
        var uniqueText = $"no-message-{Guid.NewGuid()}";

        // Act
        var received = await WaitHelper.WaitUntilAsync(
            () => Probe.Messages.OfType<SampleMessage>().Any(m => m.Text == uniqueText),
            timeout: TimeSpan.FromMilliseconds(500));

        // Assert
        received.ShouldBeFalse("Predicate should remain false when no message is published.");
    }

    [Fact]
    public async Task ConsumeAsync_ShouldConsumeMultipleMessages()
    {
        // Arrange
        var baseText = $"batch-{Guid.NewGuid()}";
        var payloads = Enumerable.Range(1, 5)
            .Select(i => new SampleMessage($"{baseText}-{i}"))
            .ToArray();

        foreach (var msg in payloads)
            await Publisher.PublishAsync(msg);

        // Act
        var receivedAll = await WaitHelper.WaitUntilAsync(
            () =>
                Probe.Messages.OfType<SampleMessage>()
                    .Select(m => m.Text)
                    .Where(t => t.StartsWith(baseText + "-", StringComparison.Ordinal))
                    .Distinct()
                    .Count() == payloads.Length,
            timeout: TimeSpan.FromSeconds(10));

        // Assert
        receivedAll.ShouldBeTrue("Not all batch messages were received within the timeout period.");
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleConcurrentPublishes()
    {
        // Arrange
        var texts = Enumerable.Range(1, 10)
            .Select(_ => $"concurrent-{Guid.NewGuid()}")
            .ToArray();

        // Act
        await Task.WhenAll(texts.Select(t => Publisher.PublishAsync(new SampleMessage(t))));

        var receivedAll = await WaitHelper.WaitUntilAsync(
            () => texts.All(t => Probe.Messages.OfType<SampleMessage>().Any(m => m.Text == t)),
            timeout: TimeSpan.FromSeconds(10));

        // Assert
        receivedAll.ShouldBeTrue("Not all concurrently published messages were received.");
    }

    [Fact]
    public async Task PublishAsync_ShouldPublishAndConsumeLargeMessage()
    {
        // Arrange
        var largeText = new string('x', 10_000);
        var message = new SampleMessage(largeText);

        // Act
        await Should.NotThrowAsync(() => Publisher.PublishAsync(message));

        var received = await WaitHelper.WaitUntilAsync(
            () => Probe.Messages.OfType<SampleMessage>().Any(m => m.Text.Length == largeText.Length),
            timeout: TimeSpan.FromSeconds(5));

        // Assert
        received.ShouldBeTrue("Large message was not received.");
    }
}
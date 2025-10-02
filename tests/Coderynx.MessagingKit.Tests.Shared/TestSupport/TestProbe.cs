using System.Collections.Concurrent;

namespace Coderynx.MessagingKit.Tests.Shared.TestSupport;

public sealed class TestProbe
{
    private readonly ConcurrentBag<object> _messages = [];

    public IReadOnlyCollection<object> Messages => _messages.ToArray();

    public void Record(object message)
    {
        _messages.Add(message);
    }
}
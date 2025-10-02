namespace Coderynx.MessagingKit.Tests.Shared.TestSupport;

public static class WaitHelper
{
    public static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout, TimeSpan? pollInterval = null)
    {
        var start = DateTime.UtcNow;
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(50);

        while (DateTime.UtcNow - start < timeout)
        {
            if (condition()) return true;
            await Task.Delay(interval);
        }

        return condition();
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coderynx.MessagingKit;

public static class HostConfiguration
{
    public static void UseMessaging(this IHost host, bool waitForInitialization = false)
    {
        var busProvider = host.Services.GetRequiredService<MessageBusManager>();
        busProvider.InitializeBuses();

        if (waitForInitialization)
        {
            busProvider.WaitForBusesInitialization();
        }
    }
}
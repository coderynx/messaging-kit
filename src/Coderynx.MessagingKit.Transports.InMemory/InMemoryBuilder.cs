using Coderynx.MessagingKit.Abstractions;
using Coderynx.MessagingKit.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit.Transports.InMemory;

public sealed class InMemoryBuilder : BusBuilderBase
{
    private string _busName = "default";

    internal InMemoryBuilder(IServiceCollection services) : base(services)
    {
    }

    public void WithBusName(string name)
    {
        _busName = name;
    }

    internal InMemoryOptions Build()
    {
        if (string.IsNullOrWhiteSpace(_busName))
        {
            throw new MessagingKitBusConfigurationException(nameof(_busName));
        }

        return new InMemoryOptions(_busName, MessageRegistrations);
    }
}
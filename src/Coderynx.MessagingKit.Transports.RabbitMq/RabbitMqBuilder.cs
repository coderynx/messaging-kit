using Coderynx.MessagingKit.Abstractions;
using Coderynx.MessagingKit.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit.Transports.RabbitMq;

public sealed class RabbitMqBuilder : BusBuilderBase
{
    private string _busName = "default";
    private string _clientName = string.Empty;
    private Uri? _uri;

    internal RabbitMqBuilder(IServiceCollection services) : base(services)
    {
    }

    public void WithBusName(string name)
    {
        _busName = name;
    }

    public void WithUri(Uri uri)
    {
        _uri = uri;
    }

    public void WithClientName(string clientName)
    {
        _clientName = clientName;
    }

    internal RabbitMqOptions Build()
    {
        if (_uri is null)
        {
            throw new MessagingKitBusConfigurationException(nameof(_uri));
        }

        return new RabbitMqOptions(_busName, _uri, _clientName, MessageRegistrations);
    }
}
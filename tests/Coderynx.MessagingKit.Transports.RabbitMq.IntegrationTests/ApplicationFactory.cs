using Coderynx.MessagingKit.Tests.Shared.Consumers;
using Coderynx.MessagingKit.Tests.Shared.TestSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.RabbitMq;

namespace Coderynx.MessagingKit.Transports.RabbitMq.IntegrationTests;

public sealed class ApplicationFactory : WebApplicationFactory<ApplicationFactory>, IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer = new Testcontainers.RabbitMq.RabbitMqBuilder()
        .Build();

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _rabbitMqContainer.StopAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var appBuilder = WebApplication.CreateBuilder();

        appBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:MessageBus"] = _rabbitMqContainer.GetConnectionString()
        });

        appBuilder.AddMessaging(messaging =>
        {
            messaging.AddRabbitMq(rabbitMq =>
            {
                var connectionString = appBuilder.Configuration.GetConnectionString("MessageBus");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ApplicationException("MessageBus connection string is not set");
                }

                var uri = new Uri(connectionString);

                rabbitMq.WithClientName("test-client");
                rabbitMq.WithBusName("test-bus");
                rabbitMq.WithUri(uri);

                rabbitMq.WithEvent<SampleMessage>();
            });
        });

        appBuilder.Services.AddSingleton<TestProbe>();

        appBuilder.Services.RemoveAll<IHostedService>();
        appBuilder.WebHost.UseTestServer();

        var app = appBuilder.Build();

        app.UseMessaging(true);

        _ = app.RunAsync();

        return app;
    }
}
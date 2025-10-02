using Coderynx.MessagingKit.Tests.Shared.Consumers;
using Coderynx.MessagingKit.Tests.Shared.TestSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Coderynx.MessagingKit.Transports.InMemory.IntegrationTests;

public sealed class ApplicationFactory : WebApplicationFactory<ApplicationFactory>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var appBuilder = WebApplication.CreateBuilder();

        appBuilder.AddMessaging(messaging =>
        {
            messaging.AddInMemory(rabbitMq =>
            {
                rabbitMq.WithBusName("test-bus");
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
using AutoFixture;
using Coderynx.MessagingKit.Abstractions;
using Coderynx.MessagingKit.Tests.Shared.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit.Transports.RabbitMq.IntegrationTests;

public abstract class IntegrationTestsBase(ApplicationFactory applicationFactory) : IClassFixture<ApplicationFactory>
{
    protected readonly IFixture Fixture = new Fixture();

    protected readonly IMessagePublisher Publisher = applicationFactory.Services
        .CreateScope()
        .ServiceProvider
        .GetRequiredService<IMessagePublisher>();

    protected TestProbe Probe => applicationFactory.Services.GetRequiredService<TestProbe>();
}
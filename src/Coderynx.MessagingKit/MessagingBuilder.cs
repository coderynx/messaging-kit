using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MessagingKit;

public sealed class MessagingBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
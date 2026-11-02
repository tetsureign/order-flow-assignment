using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using OrderFlow.Messaging.Consumers;
using OrderFlow.Messaging.Outbox;

namespace OrderFlow.Messaging.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddOutboxPublisher<TDbContext>(
        this IServiceCollection services,
        string topicConfigKey)
        where TDbContext : DbContext, IOutboxDbContext
    {
        services.AddHostedService(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<OutboxPublisher<TDbContext>>>();

            return new OutboxPublisher<TDbContext>(scopeFactory, configuration, topicConfigKey, logger);
        });
        return services;
    }

    public static IServiceCollection AddPulsarEventConsumer<TEvent, THandler, TDbContext>(
        this IServiceCollection services,
        string topicConfigKey,
        string subscriptionName)
        where TEvent : class, IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
        where TDbContext : DbContext, IOutboxDbContext
    {
        services.AddScoped<THandler>();

        services.AddScoped<IOutbox, Outbox<TDbContext>>();

        services.AddHostedService(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<PulsarEventConsumer<TEvent, THandler, TDbContext>>>();

            return new PulsarEventConsumer<TEvent, THandler, TDbContext>(
                scopeFactory,
                configuration,
                topicConfigKey,
                subscriptionName,
                logger);
        });

        return services;
    }
}

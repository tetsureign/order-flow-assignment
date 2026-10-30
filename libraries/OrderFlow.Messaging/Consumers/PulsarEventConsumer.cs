using System;
using System.Text.Json;
using DotPulsar;
using DotPulsar.Extensions;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using OrderFlow.Messaging.Entities;

namespace OrderFlow.Messaging.Consumers;

public class PulsarEventConsumer<TEvent, THandler, TDbContext>(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    string topicConfigKey,
    string subscriptionName,
    ILogger<PulsarEventConsumer<TEvent, THandler, TDbContext>> logger) : BackgroundService
    where TEvent : class, IIntegrationEvent
    where THandler : IIntegrationEventHandler<TEvent>
    where TDbContext : DbContext, IOutboxDbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var serviceUrl = configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650";
        var topic = configuration[topicConfigKey]!;

        await using var client = PulsarClient.Builder()
            .ServiceUrl(new Uri(serviceUrl))
            .Build();

        await using var consumer = client.NewConsumer(Schema.String)
            .Topic(topic)
            .SubscriptionName(subscriptionName)
            .SubscriptionType(SubscriptionType.KeyShared)
            .Create();

        await foreach (var message in consumer.Messages(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                var @event = JsonSerializer.Deserialize<TEvent>(message.Value());
                if (@event is null)
                {
                    await consumer.Acknowledge(message, stoppingToken);
                    continue;
                }

                var isProcessed = await db.InboxMessages.AnyAsync(m => m.EventId == @event.EventId, stoppingToken);
                if (isProcessed)
                {
                    await consumer.Acknowledge(message, stoppingToken);
                    continue;
                }

                await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);

                await handler.HandleAsync(@event, stoppingToken);

                db.InboxMessages.Add(new InboxMessage
                {
                    EventId = @event.EventId,
                    ProcessedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync(stoppingToken);
                await tx.CommitAsync(stoppingToken);

                await consumer.Acknowledge(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message {MessageId} for {EventType}", message.MessageId, typeof(TEvent).Name);
            }
        }
    }
}

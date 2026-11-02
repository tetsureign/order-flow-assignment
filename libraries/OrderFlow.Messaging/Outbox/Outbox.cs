using System.Text.Json;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using OrderFlow.Messaging.Entities;

namespace OrderFlow.Messaging.Outbox;

public class Outbox<TDbContext>(TDbContext db, IConfiguration configuration) : IOutbox where TDbContext : IOutboxDbContext
{
    private readonly string _defaultTopic = configuration["Pulsar:PublishTopic"]
        ?? throw new InvalidOperationException("Pulsar:PublishTopic is not configured.");

    public void Enqueue<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
    {
        db.OutboxMessages.Add(new OutboxMessage
        {
            EventId = @event.EventId,
            Topic = _defaultTopic,
            PartitionKey = @event.CorrelationId.ToString(),
            Payload = JsonSerializer.Serialize(@event),
            CreatedAt = DateTime.UtcNow
        });
    }
}

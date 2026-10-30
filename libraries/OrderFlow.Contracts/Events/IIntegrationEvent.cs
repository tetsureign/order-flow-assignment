using System;

namespace OrderFlow.Contracts.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    Guid CorrelationId { get; }
    DateTimeOffset Timestamp { get; }
}

using OrderFlow.Contracts.Events.Shared;

namespace OrderFlow.Contracts.Events;

public sealed record OrderPlaced(
    Guid EventId,
    Guid OrderId,
    DateTimeOffset Timestamp,
    string CustomerId,
    IReadOnlyList<OrderLineItem> Lines,
    decimal TotalAmount)
{
    public Guid CorrelationId => OrderId;
}
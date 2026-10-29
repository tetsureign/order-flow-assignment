using OrderFlow.Contracts.Events.Shared;

namespace OrderFlow.Contracts.Events;

public sealed record StockReleased(
    Guid EventId,
    Guid OrderId,
    IReadOnlyList<OrderLineItem> ReleasedLines)
{
    public Guid CorrelationId => OrderId;
}
using OrderFlow.Contracts.Events.Shared;

namespace OrderFlow.Contracts.Events;

public sealed record ReservationSucceeded(
    Guid EventId,
    Guid OrderId,
    long ReservationId,
    DateTimeOffset Timestamp,
    IReadOnlyList<OrderLineItem> Lines,
    decimal TotalAmount
)
{
    public Guid CorrelationId => OrderId;
}

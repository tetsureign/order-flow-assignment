namespace OrderFlow.Contracts.Events;

public sealed record ReservationFailed(
    Guid EventId,
    Guid OrderId,
    string Reason)
{
    public Guid CorrelationId => OrderId;
}
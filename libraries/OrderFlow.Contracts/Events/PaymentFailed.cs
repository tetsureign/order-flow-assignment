namespace OrderFlow.Contracts.Events;

public sealed record PaymentFailed(
    Guid EventId,
    Guid OrderId,
    string Reason)
{
    public Guid CorrelationId => OrderId;
}
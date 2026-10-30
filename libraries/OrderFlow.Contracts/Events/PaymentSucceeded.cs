namespace OrderFlow.Contracts.Events;

public sealed record PaymentSucceeded(
    Guid EventId,
    Guid OrderId,
    Guid PaymentId,
    decimal Amount)
{
    public Guid CorrelationId => OrderId;
}
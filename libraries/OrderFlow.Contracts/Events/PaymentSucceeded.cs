namespace OrderFlow.Contracts.Events;

public sealed record PaymentSucceeded(
    Guid EventId,
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    DateTimeOffset Timestamp) : IIntegrationEvent
{
    public Guid CorrelationId => OrderId;
}

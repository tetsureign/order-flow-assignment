namespace OrderFlow.Contracts.Events;

public sealed record PaymentFailed(
    Guid EventId,
    Guid OrderId,
    DateTimeOffset Timestamp,
    string Reason) : IIntegrationEvent
{
    public Guid CorrelationId => OrderId;
}

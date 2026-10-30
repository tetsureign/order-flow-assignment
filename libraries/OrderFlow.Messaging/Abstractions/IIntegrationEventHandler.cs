using System;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Messaging.Abstractions;

public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}

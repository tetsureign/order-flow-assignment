using System;
using OrderFlow.Contracts.Events;

namespace OrderFlow.Messaging.Abstractions;

public interface IOutbox
{
    void Enqueue<TEvent>(TEvent @event) where TEvent : IIntegrationEvent;
}

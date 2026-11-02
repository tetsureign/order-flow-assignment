using System;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using Orders.Service.Data;
using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.Handlers;

public class ReservationFailedHandler(ApplicationDbContext db) : IIntegrationEventHandler<ReservationFailed>
{
    public async Task HandleAsync(ReservationFailed @event, CancellationToken stoppingToken)
    {
        var order = await db.Orders.FindAsync([@event.OrderId], stoppingToken)
            ?? throw new InvalidOperationException($"Order with ID {@event.OrderId} not found.");

        if (!order.TryTransitionTo(OrderStatus.Cancelled))
            throw new InvalidOperationException($"Cannot transition order {@event.OrderId} to Cancelled.");

        await db.SaveChangesAsync(stoppingToken);
    }
}

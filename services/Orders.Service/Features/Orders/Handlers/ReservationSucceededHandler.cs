using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using Orders.Service.Data;
using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.Handlers;

public class ReservationSucceededHandler(ApplicationDbContext db) : IIntegrationEventHandler<ReservationSucceeded>
{
    public async Task HandleAsync(ReservationSucceeded @event, CancellationToken stoppingToken)
    {
        var order = await db.Orders.FindAsync([@event.OrderId], stoppingToken)
            ?? throw new InvalidOperationException($"Order with ID {@event.OrderId} not found.");

        if (!order.TryTransitionTo(OrderStatus.Charging))
            throw new InvalidOperationException($"Cannot transition order {@event.OrderId} to Charging.");

        order.SagaState?.ReservationCompleted = true;

        await db.SaveChangesAsync(stoppingToken);
    }
}

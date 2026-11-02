using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using Orders.Service.Data;
using Orders.Service.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Orders.Service.Features.Orders.Handlers;

public class PaymentSucceededHandler(ApplicationDbContext db) : IIntegrationEventHandler<PaymentSucceeded>
{
    public async Task HandleAsync(PaymentSucceeded @event, CancellationToken stoppingToken)
    {
        var order = await db.Orders
            .Include(o => o.SagaState)
            .FirstOrDefaultAsync(o => o.Id == @event.OrderId, stoppingToken)
            ?? throw new InvalidOperationException($"Order with ID {@event.OrderId} not found.");

        if (!order.TryTransitionTo(OrderStatus.Confirmed))
            throw new InvalidOperationException($"Cannot transition order {@event.OrderId} to Confirmed.");

        order.SagaState?.PaymentCompleted = true;

        await db.SaveChangesAsync(stoppingToken);
    }
}

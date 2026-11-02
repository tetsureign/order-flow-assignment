using System.Text.Json;
using FastEndpoints;
using OrderFlow.Contracts.Events;
using OrderFlow.Contracts.Events.Shared;
using Orders.Service.Data;
using Orders.Service.Data.Entities;
using OrderFlow.Messaging.Entities;
using OrderFlow.Messaging.Abstractions;

namespace Orders.Service.Features.Orders.Place;

public class PlaceOrderEndpoint(ApplicationDbContext db, IOutbox outbox) : Endpoint<PlaceOrderRequest, PlaceOrderResponse>
{
    public override void Configure()
    {
        Post("/orders");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PlaceOrderRequest req, CancellationToken ct)
    {
        var order = new Data.Entities.Order
        {
            CustomerId = req.CustomerId,
            TotalAmount = req.Lines.Sum(i => i.Quantity * i.UnitPrice),
            Status = OrderStatus.Reserving,
            OrderLines = req.Lines.Select(i => new OrderLine
            {
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            SagaState = new OrderSagaState
            {
                ReservationCompleted = false,
                PaymentCompleted = false
            }
        };

        var timestamp = DateTimeOffset.UtcNow;

        var orderPlacedEvent = new OrderPlaced(EventId: Guid.NewGuid(),
            OrderId: order.Id,
            Timestamp: timestamp,
            CustomerId: order.CustomerId,
            Lines: order.OrderLines
                .Select(line => new OrderLineItem(line.Sku, line.Quantity))
                .ToList(),
            TotalAmount: order.TotalAmount);

        outbox.Enqueue(orderPlacedEvent);

        db.Add(order);
        await db.SaveChangesAsync(ct);

        await Send.AcceptedAtAsync($"/orders/{order.Id}",
            new PlaceOrderResponse
            {
                OrderId = order.Id.ToString(),
                CorrelationId = order.Id.ToString(),
                Status = order.Status.ToString()
            },
            cancellation: ct
        );
    }
}

using System.Text.Json;
using FastEndpoints;
using Orders.Service.Data;
using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.Place;

public class PlaceOrderEndpoint(ApplicationDbContext context) : Endpoint<PlaceOrderRequest, PlaceOrderResponse>
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
            Status = OrderStatus.Pending,
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

        var orderPlacedEvent = new
        {
            EventType = "OrderPlaced",
            OrderId = order.Id,
            CorrelationId = order.Id,
            Timestamp = DateTime.UtcNow,
            order.CustomerId,
            order.TotalAmount,
            Lines = order.OrderLines.Select(l => new { l.Sku, l.Quantity })
        };

        var outboxMessage = new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            Topic = "orders.events",
            Payload = JsonSerializer.Serialize(orderPlacedEvent),
            CreatedAt = DateTime.UtcNow
        };

        context.Add(order);
        context.Add(outboxMessage);
        await context.SaveChangesAsync(ct);

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

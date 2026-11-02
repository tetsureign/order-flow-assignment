using System.Text.Json;
using Inventory.Service.Data;
using Inventory.Service.Data.Entities;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using OrderFlow.Messaging.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Features.Reservation.Handlers;

public class OrderPlacedHandler(ApplicationDbContext db, IOutbox outbox) : IIntegrationEventHandler<OrderPlaced>
{
    public async Task HandleAsync(OrderPlaced @event, CancellationToken stoppingToken)
    {
        var skus = @event.Lines.Select(line => line.Sku).Distinct().ToList();

        var stockItems = await db.StockItems
            .Where(s => skus.Contains(s.Sku))
            .ToDictionaryAsync(s => s.Sku, stoppingToken);

        var isStockSufficient = true;
        string? failureReason = null;

        foreach (var line in @event.Lines)
        {
            if (!stockItems.TryGetValue(line.Sku, out var stockItem))
            {
                isStockSufficient = false;
                failureReason = $"Stock item with SKU {line.Sku} not found.";
                break;
            }

            if (stockItem.QuantityOnHand - stockItem.QuantityReserved < line.Quantity)
            {
                isStockSufficient = false;
                failureReason = $"Insufficient stock for SKU {line.Sku}. Requested: {line.Quantity}, Available: {stockItem.QuantityOnHand - stockItem.QuantityReserved}.";
                break;
            }
        }

        var now = DateTime.UtcNow;

        if (isStockSufficient)
        {
            var reservations = new List<Data.Entities.Reservation>();

            foreach (var line in @event.Lines)
            {
                var stockItem = stockItems[line.Sku];
                stockItem.QuantityReserved += line.Quantity;

                var reservation = new Data.Entities.Reservation
                {
                    OrderId = @event.OrderId,
                    Sku = line.Sku,
                    Quantity = line.Quantity,
                    Status = ReservationStatus.Active,
                    CreatedAt = now
                };

                reservations.Add(reservation);
                db.Reservations.Add(reservation);
            }

            await db.SaveChangesAsync(stoppingToken);

            var firstReservationId = reservations.First().Id;

            var reservationSucceeded = new ReservationSucceeded
            (
                EventId: Guid.NewGuid(),
                OrderId: @event.OrderId,
                ReservationId: firstReservationId,
                Timestamp: now,
                Lines: @event.Lines,
                TotalAmount: @event.TotalAmount
            );

            outbox.Enqueue(reservationSucceeded);
        }
        else
        {
            var reservationFailed = new ReservationFailed
            (
                EventId: Guid.NewGuid(),
                OrderId: @event.OrderId,
                Reason: failureReason ?? "Insufficient stock",
                Timestamp: now
            );

            outbox.Enqueue(reservationFailed);
        }
    }
}

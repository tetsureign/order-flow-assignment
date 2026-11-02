using Inventory.Service.Data;
using Inventory.Service.Data.Entities;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Features.Stock.Handlers;

public class PaymentFailedHandler(ApplicationDbContext db) : IIntegrationEventHandler<PaymentFailed>
{
    public async Task HandleAsync(PaymentFailed @event, CancellationToken stoppingToken)
    {
        var reservations = await db.Reservations
            .Where(r => r.OrderId == @event.OrderId && r.Status == ReservationStatus.Active)
            .ToListAsync(stoppingToken);

        if (reservations.Count == 0) return;

        var skus = reservations.Select(r => r.Sku).ToList();
        var stockItems = await db.StockItems
            .Where(s => skus.Contains(s.Sku))
            .ToDictionaryAsync(s => s.Sku, stoppingToken);

        foreach (var reservation in reservations)
        {
            if (stockItems.TryGetValue(reservation.Sku, out var stockItem))
            {
                stockItem.QuantityReserved -= reservation.Quantity;
            }

            reservation.Status = ReservationStatus.Released;
        }

        await db.SaveChangesAsync(stoppingToken);
    }
}

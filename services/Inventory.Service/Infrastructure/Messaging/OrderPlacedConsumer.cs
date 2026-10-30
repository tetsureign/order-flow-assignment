using System.Text.Json;
using DotPulsar;
using DotPulsar.Extensions;
using Inventory.Service.Data;
using Inventory.Service.Data.Entities;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.Events;

namespace Inventory.Service.Infrastructure.Messaging;

public class OrderPlacedConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<OrderPlacedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var serviceUrl = configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650";
        var orderEventsTopic = configuration["Pulsar:OrderEventsTopic"] ?? "persistent://public/default/order-events";
        var inventoryEventsTopic = configuration["Pulsar:InventoryEventsTopic"] ?? "persistent://public/default/inventory-events";
        var subscriptionName = configuration["Pulsar:SubscriptionName"] ?? "inventory-order-placed-sub";

        await using var pulsarClient = PulsarClient.Builder()
            .ServiceUrl(new Uri(serviceUrl))
            .Build();

        await using var consumer = pulsarClient.NewConsumer(Schema.String)
            .Topic(orderEventsTopic)
            .SubscriptionName(subscriptionName)
            .SubscriptionType(SubscriptionType.KeyShared)
            .Create();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var message in consumer.Messages(stoppingToken))
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var payload = message.Value();
                    var orderPlaced = JsonSerializer.Deserialize<OrderPlaced>(payload);

                    if (orderPlaced == null)
                    {
                        logger.LogWarning("Failed to deserialize OrderPlaced event: {Payload}", payload);
                        await consumer.Acknowledge(message, stoppingToken);
                        continue;
                    }

                    var alreadyProcessed = await db.InboxMessages.AnyAsync(m => m.EventId == orderPlaced.EventId, stoppingToken);

                    if (alreadyProcessed)
                    {
                        await consumer.Acknowledge(message, stoppingToken);
                        continue;
                    }

                    var skus = orderPlaced.Lines.Select(line => line.Sku).Distinct().ToList();

                    var stockItems = await db.StockItems
                        .Where(s => skus.Contains(s.Sku))
                        .ToDictionaryAsync(s => s.Sku, stoppingToken);

                    var isStockSufficient = true;
                    string? failureReason = null;

                    foreach (var line in orderPlaced.Lines)
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

                    await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);

                    if (isStockSufficient)
                    {
                        var reservations = new List<Reservation>();

                        foreach (var line in orderPlaced.Lines)
                        {
                            var stockItem = stockItems[line.Sku];
                            stockItem.QuantityReserved += line.Quantity;

                            var reservation = new Reservation
                            {
                                OrderId = orderPlaced.OrderId,
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
                            OrderId: orderPlaced.OrderId,
                            ReservationId: firstReservationId,
                            Timestamp: now,
                            Lines: orderPlaced.Lines
                        );

                        db.OutboxMessages.Add(new OutboxMessage
                        {
                            EventId = reservationSucceeded.EventId,
                            Topic = inventoryEventsTopic,
                            PartitionKey = orderPlaced.OrderId.ToString(),
                            Payload = JsonSerializer.Serialize(reservationSucceeded),
                            CreatedAt = now
                        });
                    }
                    else
                    {
                        var reservationFailed = new ReservationFailed
                        (
                            EventId: Guid.NewGuid(),
                            OrderId: orderPlaced.OrderId,
                            Reason: failureReason ?? "Insufficient stock"
                        );

                        db.OutboxMessages.Add(new OutboxMessage
                        {
                            EventId = reservationFailed.EventId,
                            Topic = inventoryEventsTopic,
                            PartitionKey = orderPlaced.OrderId.ToString(),
                            Payload = JsonSerializer.Serialize(reservationFailed),
                            CreatedAt = now
                        });
                    }

                    db.InboxMessages.Add(new InboxMessage
                    {
                        EventId = orderPlaced.EventId,
                        ProcessedAt = now
                    });

                    await db.SaveChangesAsync(stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                    await consumer.Acknowledge(message, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing OrderPlaced messages");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}

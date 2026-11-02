using System.Text.Json;
using DotPulsar;
using DotPulsar.Extensions;
using Payment.Service.Data;
using Payment.Service.Data.Entities;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.Events;
using Payment.Service.Infrastructure.Payment;
using OrderFlow.Messaging.Entities;

namespace Payment.Service.Infrastructure.Messaging;

public class ReservationSucceededConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ReservationSucceededConsumer> logger,
    IPaymentService paymentService
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var serviceUrl = configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650";
        var inventoryEventsTopic = configuration["Pulsar:InventoryEventsTopic"] ?? "persistent://public/default/inventory-events";
        var paymentEventsTopic = configuration["Pulsar:PaymentEventsTopic"] ?? "persistent://public/default/payment-events";
        var subscriptionName = configuration["Pulsar:SubscriptionName"] ?? "payment-reservation-succeeded-sub";

        await using var pulsarClient = PulsarClient.Builder()
            .ServiceUrl(new Uri(serviceUrl))
            .Build();

        await using var consumer = pulsarClient.NewConsumer(Schema.String)
            .Topic(inventoryEventsTopic)
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
                    var reservationSucceeded = JsonSerializer.Deserialize<ReservationSucceeded>(payload);

                    if (reservationSucceeded == null)
                    {
                        logger.LogWarning("Failed to deserialize ReservationSucceeded event: {Payload}", payload);
                        await consumer.Acknowledge(message, stoppingToken);
                        continue;
                    }

                    var alreadyProcessed = await db.InboxMessages.AnyAsync(m => m.EventId == reservationSucceeded.EventId, stoppingToken);

                    if (alreadyProcessed)
                    {
                        await consumer.Acknowledge(message, stoppingToken);
                        continue;
                    }

                    var paymentResult = await paymentService.ProcessPaymentAsync(reservationSucceeded.OrderId, reservationSucceeded.TotalAmount, stoppingToken);

                    var now = DateTime.UtcNow;

                    if (paymentResult)
                    {
                        var payment = new Data.Entities.Payment
                        {
                            OrderId = reservationSucceeded.OrderId,
                            Amount = reservationSucceeded.TotalAmount,
                            Status = PaymentStatus.Succeeded,
                            CreatedAt = now
                        };

                        var paymentSucceededEvent = new PaymentSucceeded
                        (
                            EventId: Guid.NewGuid(),
                            OrderId: reservationSucceeded.OrderId,
                            PaymentId: payment.Id,
                            Amount: payment.Amount,
                            Timestamp: now
                        );

                        db.Payments.Add(payment);

                        db.OutboxMessages.Add(new OutboxMessage
                        {
                            EventId = Guid.NewGuid(),
                            Payload = JsonSerializer.Serialize(paymentSucceededEvent),
                            PartitionKey = reservationSucceeded.OrderId.ToString(),
                            CreatedAt = now
                        });
                    }
                    else
                    {
                        var payment = new Data.Entities.Payment
                        {
                            OrderId = reservationSucceeded.OrderId,
                            Amount = reservationSucceeded.TotalAmount,
                            Status = PaymentStatus.Failed,
                            CreatedAt = now
                        };

                        var paymentFailedEvent = new PaymentFailed
                        (
                            EventId: Guid.NewGuid(),
                            OrderId: reservationSucceeded.OrderId,
                            Reason: "Payment processing failed",
                            Timestamp: now
                        );

                        db.Payments.Add(payment);

                        db.OutboxMessages.Add(new OutboxMessage
                        {
                            EventId = Guid.NewGuid(),
                            Payload = JsonSerializer.Serialize(paymentFailedEvent),
                            PartitionKey = reservationSucceeded.OrderId.ToString(),
                            CreatedAt = now
                        });

                        logger.LogError("Payment processing failed for OrderId: {OrderId}", reservationSucceeded.OrderId);
                        await consumer.Acknowledge(message, stoppingToken);
                        continue;
                    }

                    db.InboxMessages.Add(new InboxMessage
                    {
                        EventId = reservationSucceeded.EventId,
                        ProcessedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync(stoppingToken);
                    await consumer.Acknowledge(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing ReservationSucceeded messages");
                await Task.Delay(5000, stoppingToken); // Wait before retrying
            }
        }

    }
}

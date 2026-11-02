using System;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Abstractions;
using Payment.Service.Data;
using Payment.Service.Data.Entities;
using Payment.Service.Infrastructure.Payment;

namespace Payment.Service.Features.Payments.Handlers;

public class ReservationSucceededHandler(
    ApplicationDbContext db,
    IOutbox outbox,
    IPaymentService paymentService) : IIntegrationEventHandler<ReservationSucceeded>
{
    public async Task HandleAsync(ReservationSucceeded @event, CancellationToken stoppingToken)
    {
        var paymentResult = await paymentService.ProcessPaymentAsync(@event.OrderId, @event.TotalAmount, stoppingToken);

        var now = DateTime.UtcNow;

        if (paymentResult)
        {
            var payment = new Data.Entities.Payment
            {
                OrderId = @event.OrderId,
                Amount = @event.TotalAmount,
                Status = PaymentStatus.Succeeded,
                CreatedAt = now
            };

            var paymentSucceededEvent = new PaymentSucceeded
            (
                EventId: Guid.NewGuid(),
                OrderId: @event.OrderId,
                PaymentId: payment.Id,
                Amount: payment.Amount,
                Timestamp: now
            );

            db.Payments.Add(payment);

            outbox.Enqueue(paymentSucceededEvent);
        }
        else
        {
            var payment = new Data.Entities.Payment
            {
                OrderId = @event.OrderId,
                Amount = @event.TotalAmount,
                Status = PaymentStatus.Failed,
                CreatedAt = now
            };

            var paymentFailedEvent = new PaymentFailed
            (
                EventId: Guid.NewGuid(),
                OrderId: @event.OrderId,
                Reason: "Payment processing failed",
                Timestamp: now
            );

            db.Payments.Add(payment);

            outbox.Enqueue(paymentFailedEvent);
        }
    }
}

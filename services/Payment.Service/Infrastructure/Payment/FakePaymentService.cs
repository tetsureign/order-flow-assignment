using System;

namespace Payment.Service.Infrastructure.Payment;

public class FakePaymentService : IPaymentService
{
    public Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken)
    {
        var isPaymentSuccessful = !(amount % 1 != 0.99m);

        return Task.FromResult(isPaymentSuccessful);
    }
}

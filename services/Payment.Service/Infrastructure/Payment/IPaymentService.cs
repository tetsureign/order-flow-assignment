namespace Payment.Service.Infrastructure.Payment;

public interface IPaymentService
{
    public Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken);
}

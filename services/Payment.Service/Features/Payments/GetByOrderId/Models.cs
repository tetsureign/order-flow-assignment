namespace Payment.Service.Features.Payments.GetByOrderId;

public class GetPaymentByOrderIdRequest
{
    public Guid OrderId { get; set; }
}

public class GetPaymentByOrderIdResponse
{
    public required Guid PaymentId { get; set; }
    public required Guid OrderId { get; set; }
    public required decimal Amount { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
}

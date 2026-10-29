using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Payment.Service.Data;

namespace Payment.Service.Features.Payments.GetByOrderId;

public class GetPaymentByOrderIdEndpoint(ApplicationDbContext context) : Endpoint<GetPaymentByOrderIdRequest, GetPaymentByOrderIdResponse>
{
    public override void Configure()
    {
        Get("/payments/{orderId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPaymentByOrderIdRequest req, CancellationToken ct)
    {
        var payment = await context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == req.OrderId, ct);

        if (payment == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new GetPaymentByOrderIdResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt
        }, ct);
    }
}

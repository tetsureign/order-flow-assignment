using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Data;
using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.GetById;

public class GetOrderEndpoint(ApplicationDbContext context) : Endpoint<GetOrderByIdRequest, GetOrderByIdResponse>
{
    public override void Configure()
    {
        Get("/orders/{OrderId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetOrderByIdRequest req, CancellationToken ct)
    {
        var order = await context.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id.ToString() == req.OrderId, ct);

        if (order == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new GetOrderByIdResponse
        {
            OrderId = order.Id.ToString(),
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            Lines = order.OrderLines.Select(l => new OrderLine
            {
                Sku = l.Sku,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        await Send.OkAsync(response, ct);
    }
}

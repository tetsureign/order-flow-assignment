using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Data;
using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.GetOrders;

public class GetOrdersEndpoint(ApplicationDbContext context) : Endpoint<GetOrdersRequest, GetOrdersResponse>
{
    public override void Configure()
    {
        Get("/orders");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetOrdersRequest req, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(req.CustomerId))
        {
            var orders = await context.Orders
            .Where(o => o.CustomerId == req.CustomerId)
            .Select(o => new OrderSummary
            {
                OrderId = o.Id.ToString(),
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);

            var response = new GetOrdersResponse
            {
                Orders = orders
            };

            await Send.OkAsync(response, ct);
        }
        else
        {
            var orders = await context.Orders
            .Select(o => new OrderSummary
            {
                OrderId = o.Id.ToString(),
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);

            var response = new GetOrdersResponse
            {
                Orders = orders
            };

            await Send.OkAsync(response, ct);
        }
    }
}

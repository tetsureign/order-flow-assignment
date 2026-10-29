using FastEndpoints;
using Inventory.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Features.Stock.GetAll;

public class GetStockEndpoint(ApplicationDbContext context) : EndpointWithoutRequest<GetStockResponse>
{
    public override void Configure()
    {
        Get("/stock");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await context.StockItems
            .Select(s => new StockItemSummary
            {
                Sku = s.Sku,
                QuantityOnHand = s.QuantityOnHand,
                QuantityReserved = s.QuantityReserved
            })
            .ToListAsync(ct);

        await Send.OkAsync(new GetStockResponse { Items = items }, ct);
    }
}

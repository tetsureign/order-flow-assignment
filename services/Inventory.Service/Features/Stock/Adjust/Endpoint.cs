using FastEndpoints;
using Inventory.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Features.Stock.Adjust;

public class AdjustStockEndpoint(ApplicationDbContext context) : Endpoint<AdjustStockRequest, AdjustStockResponse>
{
    public override void Configure()
    {
        Post("/stock/{sku}/adjust");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AdjustStockRequest req, CancellationToken ct)
    {
        var item = await context.StockItems
            .FirstOrDefaultAsync(s => s.Sku == req.Sku, ct);

        if (item == null)
        {
            item = new Data.Entities.StockItem
            {
                Sku = req.Sku,
                QuantityOnHand = req.Quantity,
                QuantityReserved = 0
            };
            context.StockItems.Add(item);
        }
        else
        {
            item.QuantityOnHand += req.Quantity;
        }

        await context.SaveChangesAsync(ct);

        await Send.OkAsync(new AdjustStockResponse
        {
            Sku = item.Sku,
            QuantityOnHand = item.QuantityOnHand,
            QuantityReserved = item.QuantityReserved,
            Available = item.QuantityOnHand - item.QuantityReserved
        }, ct);
    }
}

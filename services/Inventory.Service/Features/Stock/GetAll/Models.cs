namespace Inventory.Service.Features.Stock.GetAll;

public class GetStockResponse
{
    public required List<StockItemSummary> Items { get; set; }
}

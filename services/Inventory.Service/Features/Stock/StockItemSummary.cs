namespace Inventory.Service.Features.Stock;

public class StockItemSummary
{
    public required string Sku { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int Available => QuantityOnHand - QuantityReserved;
}

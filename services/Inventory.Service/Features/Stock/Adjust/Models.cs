namespace Inventory.Service.Features.Stock.Adjust;

public class AdjustStockRequest
{
    public required string Sku { get; set; }
    public required int Quantity { get; set; }
}

public class AdjustStockResponse
{
    public required string Sku { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int Available { get; set; }
}

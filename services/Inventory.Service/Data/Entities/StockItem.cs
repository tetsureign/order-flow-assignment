namespace Inventory.Service.Data.Entities;

public class StockItem
{
    public string Sku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; } = 0;
}

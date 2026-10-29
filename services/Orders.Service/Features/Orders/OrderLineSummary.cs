namespace Orders.Service.Features.Orders;

public class OrderLineSummary
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

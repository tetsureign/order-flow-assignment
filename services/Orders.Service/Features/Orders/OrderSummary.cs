namespace Orders.Service.Features.Orders;

public class OrderSummary
{
    public required string OrderId { get; set; }
    public required string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

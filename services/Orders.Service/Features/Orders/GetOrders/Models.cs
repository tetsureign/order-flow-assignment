namespace Orders.Service.Features.Orders.GetOrders;

public class GetOrdersRequest
{
    public string CustomerId { get; set; }
}

public class OrderSummary
{
    public string OrderId { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetOrdersResponse
{
    public List<OrderSummary> Orders { get; set; }
}

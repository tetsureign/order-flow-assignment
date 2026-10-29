namespace Orders.Service.Features.Orders.GetOrders;

public class GetOrdersRequest
{
    public required string CustomerId { get; set; }
}

public class GetOrdersResponse
{
    public required List<OrderSummary> Orders { get; set; }
}

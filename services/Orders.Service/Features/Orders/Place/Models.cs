using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.Place;

public class PlaceOrderRequest
{
    public required string CustomerId { get; set; }
    public required List<OrderLineSummary> Lines { get; set; }
}

public class PlaceOrderResponse
{
    public required string OrderId { get; set; }
    public required string CorrelationId { get; set; }
    public required string Status { get; set; }
}

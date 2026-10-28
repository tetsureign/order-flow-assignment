using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.Place;

public class PlaceOrderRequest
{
    public string CustomerId { get; set; }
    public List<OrderLine> Lines { get; set; }
}

public class PlaceOrderResponse
{
    public string OrderId { get; set; }
    public string CorrelationId { get; set; }
    public string Status { get; set; }
}

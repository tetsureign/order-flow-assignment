using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.GetById;

public class GetOrderByIdRequest
{
    public required string OrderId { get; set; }
}

public class GetOrderByIdResponse
{
    public required string OrderId { get; set; }
    public required string CustomerId { get; set; }
    public required List<OrderLineSummary> Lines { get; set; }
    public required string Status { get; set; }
}

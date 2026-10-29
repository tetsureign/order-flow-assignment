using Orders.Service.Data.Entities;

namespace Orders.Service.Features.Orders.GetById;

public class GetOrderByIdRequest
{
    public string OrderId { get; set; }
}

public class GetOrderByIdResponse
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public List<OrderLine> Lines { get; set; }
    public string Status { get; set; }
}

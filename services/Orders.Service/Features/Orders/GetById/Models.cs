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
    public required string Status { get; set; }
    public required decimal TotalAmount { get; set; }
    public required bool ReservationCompleted { get; set; }
    public required bool PaymentCompleted { get; set; }

    public required List<OrderLineSummary> Lines { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

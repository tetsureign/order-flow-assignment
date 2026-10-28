namespace Orders.Service.Data.Entities;

public enum OrderStatus
{
    Pending,
    Reserving,
    Charging,
    Confirmed,
    Cancelled,
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderLine> OrderLines { get; set; } = [];
    public OrderSagaState? SagaState { get; set; }
}

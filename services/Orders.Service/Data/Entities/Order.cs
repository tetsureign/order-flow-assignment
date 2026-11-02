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

    public bool CanTransitionTo(OrderStatus nextStatus)
    {
        return Status switch
        {
            OrderStatus.Pending => nextStatus is OrderStatus.Reserving or OrderStatus.Cancelled,
            OrderStatus.Reserving => nextStatus is OrderStatus.Charging or OrderStatus.Cancelled,
            OrderStatus.Charging => nextStatus is OrderStatus.Confirmed or OrderStatus.Cancelled,
            _ => false
        };
    }

    public bool TryTransitionTo(OrderStatus nextStatus)
    {
        if (Status == OrderStatus.Confirmed || Status == OrderStatus.Cancelled)
            return false;

        if (!CanTransitionTo(nextStatus))
            throw new InvalidOperationException($"Cannot transition order {Id} from {Status} to {nextStatus}.");

        Status = nextStatus;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}

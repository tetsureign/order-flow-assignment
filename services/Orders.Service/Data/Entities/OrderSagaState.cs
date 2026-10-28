namespace Orders.Service.Data.Entities;

public class OrderSagaState
{
    public Guid OrderId { get; set; }
    public bool ReservationCompleted { get; set; } = false;
    public bool PaymentCompleted { get; set; } = false;
    public Guid? LastProcessedEventId { get; set; }

    public Order? Order { get; set; }
}

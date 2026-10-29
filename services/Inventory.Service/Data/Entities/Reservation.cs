namespace Inventory.Service.Data.Entities;

public enum ReservationStatus
{
    Active,
    Released,
    Consumed,
}

public class Reservation
{
    public long Id { get; set; }
    public Guid OrderId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

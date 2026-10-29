namespace Payment.Service.Data.Entities;

public class InboxMessage
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

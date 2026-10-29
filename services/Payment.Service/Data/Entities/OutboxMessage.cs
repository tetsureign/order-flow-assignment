namespace Payment.Service.Data.Entities;

public class OutboxMessage
{
    public long Id { get; set; }
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty; // JSON string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}

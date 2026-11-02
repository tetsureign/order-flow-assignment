namespace Payment.Service.Data;

using Microsoft.EntityFrameworkCore;
using Payment.Service.Data.Entities;
using OrderFlow.Messaging.Abstractions;
using OrderFlow.Messaging.Entities;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IOutboxDbContext
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("orderflow_payments");

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.OrderId).IsRequired();
            entity.HasIndex(p => p.OrderId).IsUnique();
            entity.Property(p => p.Amount).HasPrecision(10, 2).IsRequired();
            entity.Property(p => p.Status).IsRequired().HasConversion<string>();
            entity.Property(p => p.CreatedAt).IsRequired();
        });

        // outbox_messages
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).UseIdentityColumn();
            entity.Property(m => m.EventId).IsRequired();
            entity.HasIndex(m => m.EventId).IsUnique();
            entity.Property(m => m.PartitionKey).IsRequired();
            entity.Property(m => m.Topic).HasMaxLength(255).IsRequired();
            entity.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
            entity.Property(m => m.CreatedAt).IsRequired();
            entity.Property(m => m.PublishedAt);
        });

        // inbox_messages
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(i => i.EventId);
            entity.Property(i => i.EventId).HasColumnName("event_id");
            entity.Property(i => i.ProcessedAt).HasColumnName("processed_at");
        });
    }
}

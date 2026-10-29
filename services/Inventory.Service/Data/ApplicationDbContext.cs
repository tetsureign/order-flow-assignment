namespace Inventory.Service.Data;

using Inventory.Service.Data.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("orderflow_inventory");

        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasKey(s => s.Sku);
            entity.Property(s => s.Sku).HasMaxLength(50).IsRequired();
            entity.Property(s => s.QuantityOnHand).IsRequired();
            entity.Property(s => s.QuantityReserved).IsRequired();
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).UseIdentityColumn();
            entity.Property(r => r.OrderId).IsRequired();
            entity.Property(r => r.Sku).HasMaxLength(50).IsRequired();
            entity.Property(r => r.Quantity).IsRequired();
            entity.Property(r => r.Status).IsRequired().HasConversion<string>();
            entity.Property(r => r.CreatedAt).IsRequired();

            entity.HasIndex(r => new { r.OrderId, r.Sku }).IsUnique();
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

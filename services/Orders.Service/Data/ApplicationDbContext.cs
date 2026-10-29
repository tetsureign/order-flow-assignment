namespace Orders.Service.Data;

using Microsoft.EntityFrameworkCore;
using Orders.Service.Data.Entities;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderSagaState> OrderSagaStates => Set<OrderSagaState>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("orderflow_orders");

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerId).HasMaxLength(100).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(10, 2).IsRequired();
            entity.Property(o => o.Status).IsRequired().HasConversion<string>();
            entity.Property(o => o.CreatedAt).IsRequired();
            entity.Property(o => o.UpdatedAt).IsRequired();

            entity.HasOne(o => o.SagaState)
             .WithOne(s => s.Order)
             .HasForeignKey<OrderSagaState>(s => s.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(ol => ol.Id);
            entity.Property(ol => ol.Id).UseIdentityColumn();
            entity.Property(ol => ol.OrderId).IsRequired();
            entity.Property(ol => ol.Sku).HasMaxLength(50).IsRequired();
            entity.Property(ol => ol.Quantity).IsRequired();
            entity.Property(ol => ol.UnitPrice).HasPrecision(10, 2).IsRequired();

            entity.HasOne(ol => ol.Order)
             .WithMany(o => o.OrderLines)
             .HasForeignKey(ol => ol.OrderId);
        });

        modelBuilder.Entity<OrderSagaState>(entity =>
        {
            entity.HasKey(s => s.OrderId);
            entity.Property(s => s.OrderId);
            entity.Property(s => s.ReservationCompleted).IsRequired();
            entity.Property(s => s.PaymentCompleted).IsRequired();
            entity.Property(s => s.LastProcessedEventId);
        });

        // outbox_messages
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).UseIdentityColumn();
            entity.Property(m => m.EventId).IsRequired();
            entity.HasIndex(m => m.EventId).IsUnique();
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

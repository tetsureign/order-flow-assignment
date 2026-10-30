using Microsoft.EntityFrameworkCore;
using OrderFlow.Messaging.Entities;

namespace OrderFlow.Messaging.Abstractions;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<InboxMessage> InboxMessages { get; }
}

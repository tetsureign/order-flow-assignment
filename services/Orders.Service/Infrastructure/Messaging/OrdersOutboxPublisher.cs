using DotPulsar;
using DotPulsar.Extensions;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Data;

namespace Orders.Service.Infrastructure.Messaging;

public class OrdersOutboxPublisher(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<OrdersOutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var serviceUrl = configuration["Pulsar:ServiceUrl"] ?? "pulsar://localhost:6650";
        var topic = configuration["Pulsar:TopicName"] ?? "persistent://public/default/order-events";

        await using var pulsarClient = PulsarClient.Builder()
            .ServiceUrl(new Uri(serviceUrl))
            .Build();

        await using var producer = pulsarClient.NewProducer(Schema.String)
            .Topic(topic)
            .Create();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var messages = await db.OutboxMessages
                    .Where(m => m.PublishedAt == null)
                    .OrderBy(m => m.Id)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    var metadata = new MessageMetadata
                    {
                        Key = message.PartitionKey
                    };

                    await producer.Send(metadata, message.Payload, stoppingToken);

                    message.PublishedAt = DateTime.UtcNow;
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish outbox messages.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}

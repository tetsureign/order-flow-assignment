using FastEndpoints;
using FastEndpoints.OpenApi;
using FastEndpoints.Swagger;
using Inventory.Service.Data;
using Inventory.Service.Features.Reservation.Handlers;
using Inventory.Service.Features.Stock.Handlers;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.Events;
using OrderFlow.Messaging.Extensions;

var bld = WebApplication.CreateBuilder(args);
bld.Services
  .AddFastEndpoints()
  .OpenApiDocument();
bld.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Inventory Service";
        s.Version = "v1";
    };
});

var connectionString = bld.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
bld.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
bld.Services.AddOutboxPublisher<ApplicationDbContext>(
    topicConfigKey: "Pulsar:PublishTopic"
);
bld.Services.AddPulsarEventConsumer<OrderPlaced, OrderPlacedHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:OrderEventsTopic",
    subscriptionName: bld.Configuration["Pulsar:SubscriptionName"] ?? "inventory-subscription");
bld.Services.AddPulsarEventConsumer<PaymentSucceeded, PaymentSucceededHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:PaymentEventsTopic",
    subscriptionName: bld.Configuration["Pulsar:SubscriptionName"] ?? "inventory-subscription");
bld.Services.AddPulsarEventConsumer<PaymentFailed, PaymentFailedHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:PaymentEventsTopic",
    subscriptionName: bld.Configuration["Pulsar:SubscriptionName"] ?? "inventory-subscription");

var app = bld.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

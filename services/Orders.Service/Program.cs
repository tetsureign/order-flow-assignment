using FastEndpoints;
using FastEndpoints.OpenApi;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Data;
using OrderFlow.Messaging.Extensions;
using Orders.Service.Features.Orders.Handlers;
using OrderFlow.Contracts.Events;

var bld = WebApplication.CreateBuilder(args);
bld.Services
  .AddFastEndpoints()
  .OpenApiDocument();
bld.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Orders Service";
        s.Version = "v1";
    };
});

var connectionString = bld.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
bld.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
bld.Services.AddOutboxPublisher<ApplicationDbContext>(
    topicConfigKey: "Pulsar:PublishTopic"
);
bld.Services.AddPulsarEventConsumer<ReservationSucceeded, ReservationSucceededHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:InventoryEventsTopic",
    subscriptionName: "orders-reservation-succeeded-sub"
);
bld.Services.AddPulsarEventConsumer<ReservationFailed, ReservationFailedHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:InventoryEventsTopic",
    subscriptionName: "orders-reservation-failed-sub"
);
bld.Services.AddPulsarEventConsumer<PaymentSucceeded, PaymentSucceededHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:PaymentEventsTopic",
    subscriptionName: "orders-payment-succeeded-sub"
);
bld.Services.AddPulsarEventConsumer<PaymentFailed, PaymentFailedHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:PaymentEventsTopic",
    subscriptionName: "orders-payment-failed-sub"
);

var app = bld.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

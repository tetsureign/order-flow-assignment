using FastEndpoints;
using FastEndpoints.OpenApi;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Payment.Service.Data;
using Payment.Service.Infrastructure.Payment;
using OrderFlow.Messaging.Extensions;
using OrderFlow.Contracts.Events;
using Payment.Service.Features.Payments.Handlers;

var bld = WebApplication.CreateBuilder(args);
bld.Services
  .AddFastEndpoints()
  .OpenApiDocument();
bld.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Payment Service";
        s.Version = "v1";
    };
});

var connectionString = bld.Configuration.GetConnectionString("DefaultConnection") ??
                        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
bld.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
bld.Services.AddScoped<IPaymentService, FakePaymentService>();
bld.Services.AddOutboxPublisher<ApplicationDbContext>(
    topicConfigKey: "Pulsar:PublishTopic"
);
bld.Services.AddPulsarEventConsumer<ReservationSucceeded, ReservationSucceededHandler, ApplicationDbContext>(
    topicConfigKey: "Pulsar:InventoryEventsTopic",
    subscriptionName: bld.Configuration["Pulsar:SubscriptionName"] ?? "payment-subscription"
);

var app = bld.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

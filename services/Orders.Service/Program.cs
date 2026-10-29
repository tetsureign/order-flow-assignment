using FastEndpoints;
using FastEndpoints.OpenApi;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Data;

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

var app = bld.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

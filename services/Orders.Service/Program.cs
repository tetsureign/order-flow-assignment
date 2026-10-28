using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Data;

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();
var connectionString = bld.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
bld.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

var app = bld.Build();

app.UseFastEndpoints();
app.Run();

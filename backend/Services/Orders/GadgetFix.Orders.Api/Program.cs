using GadgetFix.Orders.Api;
using GadgetFix.Orders.BLL;
using GadgetFix.Orders.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<OrdersDbContext>("ordersdb");

builder.Services.AddScoped<IOrderService, OrderService>();

// HTTP-клієнт до сервісу нотифікацій (Aspire service discovery)
builder.Services.AddHttpClient<IOrderNotifier, HttpOrderNotifier>(client =>
{
    client.BaseAddress = new Uri("https+http://notifications-api");
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

using System.Text;
using GadgetFix.Orders.Api;
using GadgetFix.Orders.BLL;
using GadgetFix.Orders.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<OrdersDbContext>("ordersdb");

// JWT (той самий ключ, що й у Users-сервісі)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-super-secret-key-change-me-please-32+chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GadgetFix";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GadgetFix";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderNotifier, HttpOrderNotifier>();

// HTTP-клієнти до інших сервісів (Aspire service discovery)
builder.Services.AddHttpClient("notifications", c => c.BaseAddress = new Uri("https+http://notifications-api"));
builder.Services.AddHttpClient("users", c => c.BaseAddress = new Uri("https+http://users-api"));

// gRPC-клієнт до сервісу каталогу (міжсервісна комунікація через gRPC)
builder.Services.AddGrpcClient<GadgetFix.Catalog.Grpc.CatalogGrpc.CatalogGrpcClient>(o =>
    o.Address = new Uri("https://catalog-api"))
    .AddServiceDiscovery();

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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

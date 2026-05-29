using GadgetFix.Users.BLL.Services;
using GadgetFix.Users.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// PostgreSQL через Aspire (connection string "usersdb")
builder.AddNpgsqlDbContext<UsersDbContext>("usersdb");

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Автоматичне застосування міграцій при старті
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

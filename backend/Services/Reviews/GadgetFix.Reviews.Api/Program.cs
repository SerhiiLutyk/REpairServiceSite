using System.Text;
using GadgetFix.Reviews.BLL;
using GadgetFix.Reviews.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<ReviewsDbContext>("reviewsdb");

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-super-secret-key-change-me-please-32+chars";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = "GadgetFix", ValidAudience = "GadgetFix",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<ReviewsDbContext>().Database.MigrateAsync();

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

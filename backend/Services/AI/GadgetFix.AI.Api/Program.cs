using GadgetFix.AI.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<PriceEstimator>();
builder.Services.AddHttpClient<GroqEstimator>();
builder.Services.AddHttpClient<GeminiEstimator>();
builder.Services.AddScoped<IEstimateService, EstimateService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

using GadgetFix.Notifications.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var telegramOptions = builder.Configuration.GetSection("Telegram").Get<TelegramOptions>() ?? new TelegramOptions();
builder.Services.AddSingleton(telegramOptions);
builder.Services.AddHttpClient<ITelegramSender, TelegramSender>();

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

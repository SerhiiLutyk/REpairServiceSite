using GadgetFix.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(new BotOptions
{
    Token = builder.Configuration["Telegram:BotToken"] ?? "",
});

// HTTP-клієнти до інших мікросервісів (Aspire service discovery)
builder.Services.AddHttpClient("users", c => c.BaseAddress = new Uri("https+http://users-api"));
builder.Services.AddHttpClient("orders", c => c.BaseAddress = new Uri("https+http://orders-api"));
builder.Services.AddHttpClient("ai", c => c.BaseAddress = new Uri("https+http://ai-api"));
builder.Services.AddHttpClient("telegram");

builder.Services.AddSingleton<BotBackend>();
builder.Services.AddSingleton<TelegramClient>();
builder.Services.AddHostedService<BotWorker>();

var host = builder.Build();
host.Run();

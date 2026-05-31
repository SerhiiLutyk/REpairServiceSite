var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

const string FrontendCors = "frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true));
});

// YARP reverse proxy з резолвом адрес через Aspire service discovery
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors(FrontendCors);
app.MapReverseProxy();

app.Run();

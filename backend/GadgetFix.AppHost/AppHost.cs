var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL у контейнері (Docker) з веб-адмінкою pgAdmin
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var usersDb = postgres.AddDatabase("usersdb");
var catalogDb = postgres.AddDatabase("catalogdb");
var ordersDb = postgres.AddDatabase("ordersdb");

// Мікросервіси
var users = builder.AddProject<Projects.GadgetFix_Users_Api>("users-api")
    .WithReference(usersDb)
    .WaitFor(usersDb);

var catalog = builder.AddProject<Projects.GadgetFix_Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

var notifications = builder.AddProject<Projects.GadgetFix_Notifications_Api>("notifications-api");

var ai = builder.AddProject<Projects.GadgetFix_AI_Api>("ai-api");

var orders = builder.AddProject<Projects.GadgetFix_Orders_Api>("orders-api")
    .WithReference(ordersDb)
    .WithReference(notifications)
    .WaitFor(ordersDb);

// API Gateway (YARP) — єдина точка входу для фронтенду
builder.AddProject<Projects.GadgetFix_ApiGateway>("gateway")
    .WithExternalHttpEndpoints()
    .WithReference(users)
    .WithReference(catalog)
    .WithReference(orders)
    .WithReference(ai)
    .WithReference(notifications);

builder.Build().Run();

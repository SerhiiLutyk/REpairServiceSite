var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL у контейнері (Docker) з веб-адмінкою pgAdmin.
// Без WithDataVolume — БД ініціалізується чистою при кожному запуску,
// що уникає конфлікту згенерованого пароля зі старим томом.
var postgres = builder.AddPostgres("postgres")
    .WithImageTag("17.6-bookworm")
    .WithPgAdmin();

var usersDb = postgres.AddDatabase("usersdb");
var catalogDb = postgres.AddDatabase("catalogdb");
var ordersDb = postgres.AddDatabase("ordersdb");
var reviewsDb = postgres.AddDatabase("reviewsdb");

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
    .WithReference(users)
    .WithReference(catalog)
    .WaitFor(ordersDb);

var reviews = builder.AddProject<Projects.GadgetFix_Reviews_Api>("reviews-api")
    .WithReference(reviewsDb)
    .WaitFor(reviewsDb);

// Telegram-бот (long polling) — спілкується з Users / Orders / AI
builder.AddProject<Projects.GadgetFix_Bot>("bot")
    .WithReference(users)
    .WithReference(orders)
    .WithReference(ai);

// API Gateway (YARP) — єдина точка входу для фронтенду
builder.AddProject<Projects.GadgetFix_ApiGateway>("gateway")
    .WithExternalHttpEndpoints()
    .WithReference(users)
    .WithReference(catalog)
    .WithReference(orders)
    .WithReference(ai)
    .WithReference(notifications)
    .WithReference(reviews);

builder.Build().Run();

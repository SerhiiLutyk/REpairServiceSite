var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL у контейнері (Docker) з веб-адмінкою pgAdmin.
// Фіксований пароль + том даних: дані зберігаються між запусками,
// а сталий пароль уникає конфлікту зі збереженим томом.
var pgPassword = builder.AddParameter("pg-password", "Gadgetfix_2026", secret: true);
var postgres = builder.AddPostgres("postgres", password: pgPassword)
    .WithImageTag("17.6-bookworm")
    .WithDataVolume()
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

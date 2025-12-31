var builder = DistributedApplication.CreateBuilder(args);

// Database
var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("shopdb");

// Backend API
var api = builder.AddProject<Projects.ReSys_Api>("api")
    .WithReference(db)
    .WaitFor(db);

// ML Service (Python)
var ml = builder.AddPythonApp("ml", "../../../services/ReSys.ML", "src/main.py")
    .WithHttpEndpoint(env: "PORT", port: 8000)
    .WithEnvironment("USE_MOCK_ML", "true"); // Use mock by default for speed

// Frontend - Shop (Vue)
var shop = builder.AddNpmApp("shop", "../../../apps/ReSys.Shop")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

// Frontend - Admin (Vue)
var admin = builder.AddNpmApp("admin", "../../../apps/ReSys.Admin")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

// Gateway (YARP)
builder.AddProject<Projects.ReSys_Gateway>("gateway")
    .WithReference(api)
    .WithReference(ml)
    .WithReference(shop)
    .WithReference(admin)
    .WaitFor(api)
    .WaitFor(ml)
    .WaitFor(shop)
    .WaitFor(admin);

builder.Build().Run();

using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Database
var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithDataVolume("resys-shop-data")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("shopdb");

// Backend API
var api = builder.AddProject<Projects.ReSys_Api>("api")
    .WithReference(db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db);

// ML Service (Python)
var ml = builder.AddPythonApp("ml", "../../../services/ReSys.ML", "src/main.py")
    .WithHttpEndpoint(env: "PORT", port: 8000)
    .WithEnvironment("USE_MOCK_ML", "true") // Use mock by default for speed
    .WithEnvironment("ROOT_PATH", "/ml")
    .WithEnvironment("OTEL_SERVICE_NAME", "ml")
    .WithOtlpExporter()
    .WithHttpHealthCheck("/health");

api.WithReference(ml)
   .WithEnvironment("MlSettings__ServiceUrl", "http://ml");

// Frontend - Shop (Vue)
var shop = builder.AddNpmApp("shop", "../../../apps/ReSys.Shop")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/");

// Frontend - Admin (Vue)
var admin = builder.AddNpmApp("admin", "../../../apps/ReSys.Admin")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/");

// Gateway (YARP)
builder.AddProject<Projects.ReSys_Gateway>("gateway")
    .WithReference(api)
    .WithReference(ml)
    .WithReference(shop)
    .WithReference(admin)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(api)
    .WaitFor(ml)
    .WaitFor(shop)
    .WaitFor(admin);

builder.Build().Run();
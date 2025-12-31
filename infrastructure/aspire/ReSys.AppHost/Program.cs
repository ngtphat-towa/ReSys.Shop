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

api.WithReference(ml)
   .WithEnvironment("MlSettings__ServiceUrl", ml.GetEndpoint("http"));

// Frontend - Shop (Vue)
var shop = builder.AddNpmApp("shop", "../../../apps/ReSys.Shop")
    .WithHttpEndpoint(env: "PORT");

// Frontend - Admin (Vue)
var admin = builder.AddNpmApp("admin", "../../../apps/ReSys.Admin")
    .WithHttpEndpoint(env: "PORT");

// Gateway (YARP)
builder.AddProject<Projects.ReSys_Gateway>("gateway")
    .WithReference(api)
    .WithReference(ml)
    .WithReference(shop)
    .WithReference(admin)
    .WithEnvironment("ReverseProxy__Clusters__api-cluster__Destinations__destination1__Address", api.GetEndpoint("https"))
    .WithEnvironment("ReverseProxy__Clusters__ml-cluster__Destinations__destination1__Address", ml.GetEndpoint("http"))
    .WithEnvironment("ReverseProxy__Clusters__shop-cluster__Destinations__destination1__Address", shop.GetEndpoint("http"))
    .WithEnvironment("ReverseProxy__Clusters__admin-cluster__Destinations__destination1__Address", admin.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WaitFor(api)
    .WaitFor(ml)
    .WaitFor(shop)
    .WaitFor(admin);

builder.Build().Run();

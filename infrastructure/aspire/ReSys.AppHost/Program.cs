using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Database
var dbPassword = builder.AddParameter("dbPassword", "password", secret: true);
var postgres = builder.AddPostgres("postgres", password: dbPassword)
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithContainerName("resys_shop_db")
    .WithDataVolume("pgdata")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("shopdb");

// Mail (Papercut)
var papercut = builder.AddContainer("papercut", "changemakerstudiosus/papercut-smtp")
    .WithHttpEndpoint(port: 37408, targetPort: 8080, name: "web")
    .WithEndpoint(port: 2525, targetPort: 2525, name: "smtp")
    .WithContainerName("resys_shop_mail");

// Backend API
var api = builder.AddProject<Projects.ReSys_Api>("api")
    .WithReference(db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db);

api.WithReference(papercut.GetEndpoint("smtp"))
   .WithEnvironment("Notifications__SmtpOptions__SmtpConfig__Host", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Host)}"))
   .WithEnvironment("Notifications__SmtpOptions__SmtpConfig__Port", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Port)}"));

// ML Service (Python)
var ml = builder.AddPythonApp("ml", "../../../services/ReSys.ML", "src/main.py")
    .WithHttpEndpoint(env: "PORT", port: 8000)
    .WithEnvironment("USE_MOCK_ML", "true")
    .WithEnvironment("ROOT_PATH", "/ml")
    .WithOtlpExporter()
    .WithHttpHealthCheck("/health");

api.WithReference(ml)
   .WithEnvironment("MlSettings__ServiceUrl", ml.GetEndpoint("http"));

// Frontend - Shop (Vue)
var shop = builder.AddNpmApp("shop", "../../../apps/ReSys.Shop")
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/")
    .WaitFor(api);

// Frontend - Admin (Vue)
var admin = builder.AddNpmApp("admin", "../../../apps/ReSys.Admin")
    .WithHttpEndpoint(port: 5174, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/")
    .WaitFor(api);

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
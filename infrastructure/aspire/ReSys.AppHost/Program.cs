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
var identityDb = postgres.AddDatabase("identitydb");

// Mail (Papercut)
var papercut = builder.AddContainer("papercut", "changemakerstudiosus/papercut-smtp")
    .WithHttpEndpoint(port: 37408, targetPort: 8080, name: "web")
    .WithEndpoint(port: 2525, targetPort: 2525, name: "smtp")
    .WithContainerName("resys_shop_mail");

// Identity Service
var identity = builder.AddProject<Projects.ReSys_Identity>("identity")
    .WithHttpsEndpoint(port: 5003, name: "sso")
    .WithReference(identityDb)
    .WithExternalHttpEndpoints()
    .WaitFor(identityDb);

// Self-reference for Issuer
identity.WithEnvironment("Identity__Issuer", identity.GetEndpoint("sso"));

// Backend API
var api = builder.AddProject<Projects.ReSys_Api>("api", launchProfileName: "https")
    .WithReference(db)
    .WithReference(identity)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db)
    .WaitFor(identity);

api.WithReference(papercut.GetEndpoint("smtp"))
   .WithEnvironment("Notifications__SmtpOptions__SmtpConfig__Host", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Host)}"))
   .WithEnvironment("Notifications__SmtpOptions__SmtpConfig__Port", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Port)}"))
   .WithEnvironment("Authentication__Authority", identity.GetEndpoint("sso"));

// ML Service (Python)
#pragma warning disable ASPIREHOSTINGPYTHON001
var ml = builder.AddPythonApp("ml", "../../../services/ReSys.ML", "src/main.py")
    .WithHttpEndpoint(env: "PORT", port: 8000)
    .WithEnvironment("USE_MOCK_ML", "true")
    .WithEnvironment("ROOT_PATH", "/ml")
    .WithOtlpExporter()
    .WithHttpHealthCheck("/health");
#pragma warning restore ASPIREHOSTINGPYTHON001

api.WithReference(ml)
   .WithEnvironment("ML__ServiceUrl", ml.GetEndpoint("http"));

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
    .WithReference(identity)
    .WithReference(shop)
    .WithReference(admin)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(api)
    .WaitFor(ml)
    .WaitFor(identity)
    .WaitFor(shop)
    .WaitFor(admin);

builder.Build().Run();

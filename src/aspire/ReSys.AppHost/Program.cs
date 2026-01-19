using Microsoft.Extensions.DependencyInjection;

using ReSys.Infrastructure.Ml.Options;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Shared.Constants;

var builder = DistributedApplication.CreateBuilder(args);

// --- 1. Infrastructure Services ---
builder.Services.AddHealthChecks();
builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
{
    clientBuilder.AddStandardResilienceHandler();
});

// Database (PostgreSQL) - PERSISTENT for faster test runs
var dbPassword = builder.AddParameter("dbPassword", "password", secret: true);
var postgres = builder.AddPostgres(ServiceNames.Postgres, password: dbPassword)
    .WithImage("pgvector/pgvector", "pg17")
    .WithContainerName("resys_shop_db")
    .WithDataVolume("pgdata")
    .WithLifetime(ContainerLifetime.Persistent); // ← Keeps container between runs

var db = postgres.AddDatabase(ServiceNames.Database);

// Mail (Papercut SMTP) - PERSISTENT
var papercut = builder.AddContainer(ServiceNames.Mail, "changemakerstudiosus/papercut-smtp")
    .WithHttpEndpoint(port: 37408, targetPort: 8080, name: "web")
    .WithEndpoint(port: 2525, targetPort: 2525, name: "smtp")
    .WithContainerName("resys_shop_mail")
    .WithLifetime(ContainerLifetime.Persistent); // ← Keeps container between runs

// --- 2. Core Backend Services ---

// ML Service (Python / FastAPI)
var ml = builder.AddUvicornApp(ServiceNames.Ml, "../../services/ReSys.ML", "src.main:app")
    .WithUv()
    .WithEnvironment("USE_MOCK_ML", "true")
    .WithEnvironment("ROOT_PATH", "/ml")
    .WithHttpHealthCheck("/health");

// Backend API (.NET)
var api = builder.AddProject<Projects.ReSys_Api>(ServiceNames.Api)
    .WithReference(db)
    .WithReference(ml)
    .WithReference(papercut.GetEndpoint("smtp"))
    .WithEnvironment($"{SmtpOptions.Section.Replace(":", "__")}__{nameof(SmtpConfig.Host)}",
        ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Host)}"))
    .WithEnvironment($"{SmtpOptions.Section.Replace(":", "__")}__{nameof(SmtpConfig.Port)}",
        ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Port)}"))
    .WithEnvironment($"{MlOptions.SectionName}__{nameof(MlOptions.ServiceUrl)}",
        ml.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db); // Only wait for database - ML starts in parallel

// --- 3. Frontend Applications ---

// Shop App (Vue)
var shop = builder.AddJavaScriptApp(ServiceNames.Shop, "../../apps/ReSys.Shop")
    .WithReference(api)
    .WithHttpEndpoint(targetPort: 5174, name: "http")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", endpointName: "http")
    .WaitFor(api);

// Admin App (Vue)
var admin = builder.AddJavaScriptApp(ServiceNames.Admin, "../../apps/ReSys.Admin")
    .WithReference(api)
    .WithHttpEndpoint(targetPort: 5173, name: "http")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", endpointName: "http")
    .WaitFor(api);

// --- 4. Gateway / Ingress ---

// YARP Gateway - Only wait for API, frontends can start async
builder.AddProject<Projects.ReSys_Gateway>(ServiceNames.Gateway)
    .WithReference(api)
    .WithReference(ml)
    .WithReference(shop)
    .WithReference(admin)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(api); // Only wait for API - allows parallel startup

builder.Build().Run();

public partial class Program { }
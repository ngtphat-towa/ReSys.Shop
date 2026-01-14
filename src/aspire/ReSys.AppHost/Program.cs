using Microsoft.Extensions.DependencyInjection;

using ReSys.Infrastructure.Ml.Options;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Shared.Constants;

var builder = DistributedApplication.CreateBuilder(args);

// --- 1. Infrastructure Services ---
builder.Services.AddHealthChecks();

// Database (PostgreSQL)
var dbPassword = builder.AddParameter("dbPassword", "password", secret: true);
var postgres = builder.AddPostgres(ServiceNames.Postgres, password: dbPassword)
    .WithImage("pgvector/pgvector", "pg17")
    .WithContainerName("resys_shop_db")
    .WithDataVolume("pgdata")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase(ServiceNames.Database);

// Mail (Papercut SMTP)
var papercut = builder.AddContainer(ServiceNames.Mail, "changemakerstudiosus/papercut-smtp")
    .WithHttpEndpoint(port: 37408, targetPort: 8080, name: "web")
    .WithEndpoint(port: 2525, targetPort: 2525, name: "smtp")
    .WithContainerName("resys_shop_mail");

// --- 2. Core Backend Services ---

// ML Service (Python / FastAPI) - First-class citizen support
// Aspire 13 automates health checks and Uvicorn arguments
var ml = builder.AddUvicornApp(ServiceNames.Ml, "../../services/ReSys.ML", "src.main:app")
    .WithUv()
    .WithEnvironment("USE_MOCK_ML", "true")
    .WithEnvironment("ROOT_PATH", "/ml")
    .WithHttpHealthCheck("/health");

// Backend API (.NET)
var api = builder.AddProject<Projects.ReSys_Api>(ServiceNames.Api)
    .WithReference(db)
    .WithReference(ml)
    .WithReference(papercut.GetEndpoint("smtp")) // Inject SMTP endpoint
    .WithEnvironment($"{SmtpOptions.Section.Replace(":", "__")}__{nameof(SmtpConfig.Host)}", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Host)}"))
    .WithEnvironment($"{SmtpOptions.Section.Replace(":", "__")}__{nameof(SmtpConfig.Port)}", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Port)}"))
    .WithEnvironment($"{MlOptions.SectionName}__{nameof(MlOptions.ServiceUrl)}", ml.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db)
    .WaitFor(ml);

// Identity Service (.NET)
var identity = builder.AddProject<Projects.ReSys_Identity>(ServiceNames.Identity)
    .WithReference(db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db);

// --- 3. Frontend Applications ---

// Shop App (Vue) - Polyglot support via AddJavaScriptApp
// Reference the API to automatically inject environment variables
var shop = builder.AddJavaScriptApp(ServiceNames.Shop, "../../apps/ReSys.Shop")
    .WithReference(api)
    .WithHttpEndpoint(targetPort: 5174, name: "http")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", endpointName: "http")
    .WaitFor(api);

// Admin App (Vue) - Polyglot support via AddJavaScriptApp
var admin = builder.AddJavaScriptApp(ServiceNames.Admin, "../../apps/ReSys.Admin")
    .WithReference(api)
    .WithHttpEndpoint(targetPort: 5173, name: "http")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", endpointName: "http")
    .WaitFor(api);

// --- 4. Gateway / Ingress ---

// YARP Gateway
builder.AddProject<Projects.ReSys_Gateway>(ServiceNames.Gateway)
    .WithReference(api)
    .WithReference(identity)
    .WithReference(ml)
    .WithReference(shop)
    .WithReference(admin)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(api)
    .WaitFor(identity)
    .WaitFor(ml)
    .WaitFor(shop)
    .WaitFor(admin);

builder.Build().Run();

public partial class Program { }
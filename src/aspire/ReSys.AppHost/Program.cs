using Microsoft.Extensions.DependencyInjection;

using ReSys.Infrastructure.Ml.Options;
using ReSys.Infrastructure.Notifications.Options;
using ReSys.Shared.Constants;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Database
var dbPassword = builder.AddParameter("dbPassword", "password", secret: true);
var postgres = builder.AddPostgres(ServiceNames.Postgres, password: dbPassword)
    .WithImage("pgvector/pgvector", "pg17")
    .WithContainerName("resys_shop_db")
    .WithDataVolume("pgdata")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase(ServiceNames.Database);

// Mail (Papercut)
var papercut = builder.AddContainer(ServiceNames.Mail, "changemakerstudiosus/papercut-smtp")
    .WithHttpEndpoint(port: 37408, targetPort: 8080, name: "web")
    .WithEndpoint(port: 2525, targetPort: 2525, name: "smtp")
    .WithContainerName("resys_shop_mail");

// Backend API
var api = builder.AddProject<Projects.ReSys_Api>(ServiceNames.Api)
    .WithReference(db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db);

// Identity Service
var identity = builder.AddProject<Projects.ReSys_Identity>(ServiceNames.Identity)
    .WithReference(db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(db);

api.WithReference(papercut.GetEndpoint("smtp"))
   .WithEnvironment($"{SmtpOptions.Section.Replace(":", "__")}__{nameof(SmtpConfig.Host)}", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Host)}"))
   .WithEnvironment($"{SmtpOptions.Section.Replace(":", "__")}__{nameof(SmtpConfig.Port)}", ReferenceExpression.Create($"{papercut.GetEndpoint("smtp").Property(EndpointProperty.Port)}"));

// ML Service (Python)
#pragma warning disable ASPIREHOSTINGPYTHON001
var ml = builder.AddPythonApp(ServiceNames.Ml, "../../services/ReSys.ML", "src/main.py")
    .WithHttpEndpoint(env: "PORT", port: 8000)
    .WithEnvironment("USE_MOCK_ML", "true")
    .WithEnvironment("ROOT_PATH", "/ml")
    .WithOtlpExporter()
    .WithHttpHealthCheck("/health");
#pragma warning restore ASPIREHOSTINGPYTHON001

api.WithReference(ml)
   .WithEnvironment($"{MlOptions.SectionName}__{nameof(MlOptions.ServiceUrl)}", ml.GetEndpoint("http"));

// Frontend - Shop (Vue)
var shop = builder.AddNpmApp(ServiceNames.Shop, "../../apps/ReSys.Shop")
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/")
    .WaitFor(api);

// Frontend - Admin (Vue)
var admin = builder.AddNpmApp(ServiceNames.Admin, "../../apps/ReSys.Admin")
    .WithHttpEndpoint(port: 5174, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/")
    .WaitFor(api);

// Gateway (YARP)
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
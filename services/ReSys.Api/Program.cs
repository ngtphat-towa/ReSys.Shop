using ReSys.Api;
using ReSys.Core;
using ReSys.Infrastructure;

using ReSys.Infrastructure.AI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddPostgresHealthCheck("shopdb");

var mlOptions = builder.Configuration.GetSection(MlOptions.SectionName).Get<MlOptions>();
if (mlOptions != null)
{
    builder.AddHttpHealthCheck("ml-service", mlOptions.ServiceUrl + "/health");
}

// Register Layers
builder.Services
    .AddPresentation()
    .AddCore(typeof(ReSys.Api.DependencyInjection).Assembly)
    .AddInfrastructure(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure Pipeline
app.UseInfrastructure();
app.UseCore();
app.UsePresentation();

app.Run();

public partial class Program { }

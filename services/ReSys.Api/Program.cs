using ReSys.Api;
using ReSys.Core;
using ReSys.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register Layers
builder.Services
    .AddPresentation()
    .AddCore(typeof(ReSys.Api.DependencyInjection).Assembly)
    .AddInfrastructure(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure Pipeline
app.UseInfrastructure();
app.UseCore();
app.UsePresentation();

app.Run();

public partial class Program { }

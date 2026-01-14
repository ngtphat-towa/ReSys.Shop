using Carter;
using Serilog;
using ReSys.Identity;
using ReSys.Core;
using ReSys.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddIdentityProject(builder.Configuration);

builder.Services.AddCarter();

var app = builder.Build();

// Configure Pipeline
app.UseInfrastructure();
app.UseCore();
app.UseIdentityProject();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapCarter();

app.Run();
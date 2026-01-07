using Carter;

using ReSys.Api.Infrastructure;
using ReSys.Api.Infrastructure.Documentation;
using ReSys.Api.Infrastructure.Middleware;
using ReSys.Api.Infrastructure.Serialization;
using ReSys.Core.Common.Telemetry;

using Serilog;

namespace ReSys.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.RegisterModule("Presentation", "API", s =>
        {
            s.AddCustomSerialization() // Using your extension
             .AddDocumentation()       // Using the documentation extension
             .AddCors(options =>
             {
                 options.AddDefaultPolicy(policy =>
                 {
                     policy.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                 });
             });

            s.AddCarter();

            s.AddExceptionHandler<GlobalExceptionHandler>();
            s.AddProblemDetails();
        });

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        // 1. Safety Net (Exception Handling)
        app.UseExceptionHandler();

        // 2. Structured Request Logging (Serilog)
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        // 3. Health Checks
        app.MapDefaultEndpoints();

        // 4. Normalization & Security
        app.UseMiddleware<SnakeCaseQueryMiddleware>();
        app.UseDocumentation();

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthorization();

        // 5. Execution (Vertical Slices)
        app.MapCarter();

        return app;
    }
}

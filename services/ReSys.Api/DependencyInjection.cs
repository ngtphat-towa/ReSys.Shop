using Carter;
using ReSys.Api.Infrastructure;
using ReSys.Api.Infrastructure.Documentation;
using ReSys.Api.Infrastructure.Middleware;
using ReSys.Api.Infrastructure.Serialization;

namespace ReSys.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        Serilog.Log.Information("[Presentation] Initializing API Presentation Layer...");
        services
            .AddCustomSerialization() // Using your extension
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
            
        services.AddCarter();
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseExceptionHandler();
        
        // Global Query Normalization (Must be before routing)
        app.UseMiddleware<SnakeCaseQueryMiddleware>();

        // Documentation Pipeline (OpenAPI/Scalar)
        app.UseDocumentation();

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthorization();
        app.MapCarter();

        return app;
    }
}

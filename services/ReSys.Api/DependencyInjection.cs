using Carter;
using ReSys.Api.Extensions;
using ReSys.Api.Infrastructure;
using ReSys.Api.Infrastructure.Conventions;
using Scalar.AspNetCore;

namespace ReSys.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddCustomSerialization()
            .AddDocumentation()
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
        
        // Global Query Normalization
        app.UseMiddleware<SnakeCaseQueryMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthorization();
        app.MapCarter();

        return app;
    }
}

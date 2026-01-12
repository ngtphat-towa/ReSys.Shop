using Carter;


using ReSys.Core.Common.Telemetry;
using ReSys.Identity.Infrastructure.Documentation;

namespace ReSys.Identity.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.RegisterModule("Presentation", "Identity", s =>
        {
            var modules = typeof(DependencyInjection).Assembly.GetTypes()
                .Where(t => typeof(ICarterModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ToArray();

            s.AddCarter(configurator: c => c.WithModules(modules));
            s.AddProblemDetails();
            s.AddDocumentation();
        });

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseDocumentation();
        app.MapDefaultEndpoints();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapCarter();

        return app;
    }
}

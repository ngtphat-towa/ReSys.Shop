using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Behaviors;

namespace ReSys.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }

    public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseCore(this Microsoft.AspNetCore.Builder.IApplicationBuilder app)
    {
        // Placeholder for core pipeline setup if needed in the future
        return app;
    }
}

using ReSys.Api.Infrastructure.Conventions;
using Scalar.AspNetCore;

namespace ReSys.Api.Extensions;

public static class DocumentationExtensions
{
    public static IServiceCollection AddDocumentation(this IServiceCollection services)
    {
        // Add OpenAPI with Snake Case transformers for consistent API docs
        services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<SnakeCaseOperationTransformer>();
            options.AddSchemaTransformer<SnakeCaseSchemaTransformer>();
        });

        return services;
    }

    public static WebApplication UseDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}

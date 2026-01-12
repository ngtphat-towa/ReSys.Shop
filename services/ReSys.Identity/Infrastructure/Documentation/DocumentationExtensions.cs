using Microsoft.OpenApi.Models;


using Scalar.AspNetCore;

namespace ReSys.Identity.Infrastructure.Documentation;

public static class DocumentationExtensions
{
    public static IServiceCollection AddDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<SnakeCaseOperationTransformer>();
            options.AddSchemaTransformer<SnakeCaseSchemaTransformer>();

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "ReSys Identity API";
                document.Info.Version = "v1";
                
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token"
                });

                return Task.CompletedTask;
            });
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

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using ReSys.Shared.Extensions;

namespace ReSys.Api.Infrastructure.Documentation;

/// <summary>
/// Transforms OpenAPI operation parameters (query strings, route params) to snake_case.
/// </summary>
public sealed class SnakeCaseOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Parameters == null) return Task.CompletedTask;

        foreach (var parameter in operation.Parameters)
        {
            if (parameter is OpenApiParameter concreteParam) 
            {
                // Name might be null
                if (!string.IsNullOrEmpty(concreteParam.Name) && concreteParam.Name.Any(char.IsUpper))
                {
                    concreteParam.Name = concreteParam.Name.ToSnakeCase();
                }
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Transforms OpenAPI schema properties (response bodies, request bodies) to snake_case.
/// </summary>
public sealed class SnakeCaseSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Properties == null) return Task.CompletedTask;

        // Use IOpenApiSchema interface for the dictionary values
        var snakeCaseProperties = new Dictionary<string, IOpenApiSchema>();

        foreach (var property in schema.Properties)
        {
            var snakeCaseKey = property.Key.ToSnakeCase();
            snakeCaseProperties[snakeCaseKey] = property.Value;
        }

        schema.Properties = snakeCaseProperties;
        return Task.CompletedTask;
    }
}

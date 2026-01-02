using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using ReSys.Core.Common.Helpers;

namespace ReSys.Api.Infrastructure.Conventions;

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
            // Only transform if it's not already snake_case (contains uppercase)
            if (parameter.Name.Any(char.IsUpper))
            {
                parameter.Name = NamingHelper.ToSnakeCase(parameter.Name);
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

        var snakeCaseProperties = new Dictionary<string, OpenApiSchema>();

        foreach (var property in schema.Properties)
        {
            var snakeCaseKey = NamingHelper.ToSnakeCase(property.Key);
            snakeCaseProperties[snakeCaseKey] = property.Value;
        }

        schema.Properties = snakeCaseProperties;
        return Task.CompletedTask;
    }
}

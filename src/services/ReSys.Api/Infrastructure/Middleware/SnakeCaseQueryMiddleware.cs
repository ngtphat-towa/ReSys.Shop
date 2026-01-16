using Microsoft.Extensions.Primitives;

using ReSys.Shared.Extensions;

namespace ReSys.Api.Infrastructure.Middleware;

/// <summary>
/// Middleware to convert all query string keys to snake_case.
/// This allows frontend to use snake_case (e.g. ?user_id=1) and backend to potentially map it if needed,
/// though strictly speaking .NET binds to PascalCase properties from snake_case query params automatically in newer versions or via attributes.
/// However, if we want to normalize incoming query keys:
/// </summary>
public class SnakeCaseQueryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Query.Count > 0)
        {
            var newQuery = new Dictionary<string, StringValues>();
            foreach (var key in context.Request.Query.Keys)
            {
                var newKey = key.ToSnakeCase();
                newQuery[newKey] = context.Request.Query[key];
            }

            // Replace the query collection with the snake_cased one
            context.Request.Query = new QueryCollection(newQuery);
        }

        await next(context);
    }
}

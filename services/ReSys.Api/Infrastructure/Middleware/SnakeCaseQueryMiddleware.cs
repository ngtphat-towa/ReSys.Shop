using Microsoft.Extensions.Primitives;
using ReSys.Core.Common.Helpers;

namespace ReSys.Api.Infrastructure.Middleware;

/// <summary>
/// Middleware to normalize query parameters from snake_case (e.g., page_size) 
/// or lowercase (e.g., search) to PascalCase (e.g., PageSize, Search).
/// This ensures ASP.NET Core's [AsParameters] binding works correctly with consistent naming.
/// </summary>
public class SnakeCaseQueryMiddleware
{
    private readonly RequestDelegate _next;

    public SnakeCaseQueryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Query.Count > 0)
        {
            var normalizedQuery = new Dictionary<string, StringValues>();
            bool hasChanges = false;

            foreach (var kvp in context.Request.Query)
            {
                // Normalize ALL keys to PascalCase for consistent binding
                // - snake_case keys: page_size -> PageSize
                // - lowercase keys: search -> Search
                var pascalKey = NamingHelper.ToPascalCase(kvp.Key);
                
                if (pascalKey != kvp.Key)
                {
                    hasChanges = true;
                }
                
                normalizedQuery[pascalKey] = kvp.Value;
            }

            // Only replace the query collection if we actually modified something
            // to avoid unnecessary allocations in the happy path
            if (hasChanges)
            {
                context.Request.Query = new QueryCollection(normalizedQuery);
            }
        }

        await _next(context);
    }
}

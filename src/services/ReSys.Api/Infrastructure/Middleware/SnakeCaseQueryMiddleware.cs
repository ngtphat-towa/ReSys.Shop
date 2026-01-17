using Microsoft.Extensions.Primitives;

using ReSys.Shared.Extensions;

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
    /// <summary>
    ///  DO NOT TOUCH THE CODE BELOW WITHOUT MEASURING PERFORMANCE IMPACT FIRST.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Query.Count > 0)
        {
            // 1. FAST SCAN: Check if any keys actually need normalization
            // This avoids allocating a Dictionary for the happy path (already PascalCase keys)
            bool needsNormalization = false;
            foreach (var key in context.Request.Query.Keys)
            {
                // ToPascalCase is optimized to return the original string reference 
                // if it's already PascalCase, so this check is very cheap (reference equality).
                if (!ReferenceEquals(key, key.ToPascalCase()))
                {
                    needsNormalization = true;
                    break;
                }
            }

            // 2. SLOW PATH: Only allocate if we found something to fix
            if (needsNormalization)
            {
                var normalizedQuery = new Dictionary<string, StringValues>(context.Request.Query.Count, StringComparer.Ordinal);
                
                foreach (var kvp in context.Request.Query)
                {
                    var pascalKey = kvp.Key.ToPascalCase();
                    normalizedQuery[pascalKey] = kvp.Value;
                }
                
                context.Request.Query = new QueryCollection(normalizedQuery);
            }
        }

        await _next(context);
    }
}
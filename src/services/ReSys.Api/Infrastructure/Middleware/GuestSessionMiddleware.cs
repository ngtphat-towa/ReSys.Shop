using Microsoft.AspNetCore.Http;

namespace ReSys.Api.Infrastructure.Middleware;

public class GuestSessionMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Session-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        string sessionId;

        // 1. Try get from Request
        if (context.Request.Headers.TryGetValue(HeaderName, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue))
        {
            sessionId = headerValue.ToString();
        }
        else
        {
            // 2. Generate New
            sessionId = Guid.NewGuid().ToString();
        }

        // 3. Store for downstream use
        context.Items["SessionId"] = sessionId;

        // 4. Attach to Response so client can save it
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderName))
            {
                context.Response.Headers.Append(HeaderName, sessionId);
            }
            return Task.CompletedTask;
        });

        await next(context);
    }
}

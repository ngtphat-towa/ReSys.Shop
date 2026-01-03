using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ReSys.Api.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails();

        if (exception is BadHttpRequestException badRequestEx)
        {
            problemDetails.Status = badRequestEx.StatusCode;
            problemDetails.Title = "Bad Request";
            problemDetails.Detail = badRequestEx.Message;
            
            logger.LogWarning(exception, "Bad Request: {Message}", exception.Message);
        }
        else
        {
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Server Error";
            problemDetails.Detail = exception.Message;

            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

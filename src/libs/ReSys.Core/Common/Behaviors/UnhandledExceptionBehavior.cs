using MediatR;
using Microsoft.Extensions.Logging;
using ErrorOr;

namespace ReSys.Core.Common.Behaviors;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogError(ex, "Unhandled Exception for Request {Name} {@Request}", requestName, request);

            return (dynamic)Error.Unexpected(
                code: "General.UnhandledException",
                description: $"An unhandled exception occurred for request {requestName}.");
        }
    }
}

using MediatR;

using Microsoft.Extensions.Logging;

namespace ReSys.Core.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Processing Request: {Name}", requestName);

        var response = await next();

        logger.LogInformation("Processed Request: {Name}", requestName);

        return response;
    }
}

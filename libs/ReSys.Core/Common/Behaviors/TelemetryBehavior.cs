using MediatR;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Telemetry;
using System.Diagnostics;

namespace ReSys.Core.Common.Behaviors;

public class TelemetryBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // 1. Tracing (Span)
        using var activity = TelemetryConstants.ActivitySource.StartActivity($"UseCase {requestName}");
        activity?.SetTag("usecase.name", requestName);

        // 2. Metrics (Timer)
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("[UseCase] Starting {RequestName}", requestName);

            var response = await next();

            sw.Stop();

            // 3. Record Success Metrics
            TelemetryConstants.UseCaseDuration.Record(sw.ElapsedMilliseconds, 
                new TagList { { "usecase", requestName }, { "status", "success" } });

            logger.LogInformation("[UseCase] Completed {RequestName} in {Elapsed}ms", requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            // 4. Record Error Metrics
            TelemetryConstants.UseCaseErrors.Add(1, new TagList { { "usecase", requestName } });
            TelemetryConstants.UseCaseDuration.Record(sw.ElapsedMilliseconds, 
                new TagList { { "usecase", requestName }, { "status", "error" } });

            logger.LogError(ex, "[UseCase] Failed {RequestName} after {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

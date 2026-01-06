using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReSys.Core.Common.Behaviors;
using FluentAssertions;
using Xunit;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ReSys.Core.UnitTests.Behaviors;

public class TelemetryBehaviorTests
{
    private readonly ILogger<TestRequest> _logger;
    private readonly TelemetryBehavior<TestRequest, string> _sut;
    private readonly RequestHandlerDelegate<string> _next;

    public TelemetryBehaviorTests()
    {
        _logger = Substitute.For<ILogger<TestRequest>>();
        _sut = new TelemetryBehavior<TestRequest, string>(_logger);
        _next = Substitute.For<RequestHandlerDelegate<string>>();
    }

    [Fact(DisplayName = "Handle: Should log and call next on success")]
    public async Task Handle_Success_LogsAndCallsNext()
    {
        // Arrange
        var request = new TestRequest();
        _next.Invoke().Returns(Task.FromResult("Response"));

        // Act
        var result = await _sut.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be("Response");
        await _next.Received(1).Invoke();

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Starting")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Completed")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(DisplayName = "Handle: Should log error and rethrow on exception")]
    public async Task Handle_Exception_LogsErrorAndRethrows()
    {
        // Arrange
        var request = new TestRequest();
        var exception = new Exception("Test error");
        _next.Invoke().Returns(Task.FromException<string>(exception));

        // Act
        var act = () => _sut.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Test error");
        await _next.Received(1).Invoke();

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(DisplayName = "Handle: Should start an Activity with the correct name")]
    public async Task Handle_ShouldStartActivity()
    {
        // Arrange
        var request = new TestRequest();
        _next.Invoke().Returns(Task.FromResult("Response"));
        
        Activity? startedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = a => startedActivity = a
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        await _sut.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        startedActivity.Should().NotBeNull();
        startedActivity!.OperationName.Should().Contain("UseCase TestRequest");
        startedActivity.TagObjects.Should().Contain(t => t.Key == "usecase.name" && t.Value as string == "TestRequest");
    }

    [Fact(DisplayName = "Handle: Should record duration metrics on success")]
    public async Task Handle_ShouldRecordMetrics()
    {
        // Arrange
        var request = new TestRequest();
        _next.Invoke().Returns(Task.FromResult("Response"));

        var recordedDuration = false;
        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == "ReSys.Shop" && instrument.Name == "usecase.duration")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "usecase.duration")
            {
                recordedDuration = true;
                var tagDict = new Dictionary<string, object?>();
                foreach (var tag in tags)
                {
                    tagDict[tag.Key] = tag.Value;
                }
                tagDict["usecase"].Should().Be("TestRequest");
                tagDict["status"].Should().Be("success");
            }
        });
        meterListener.Start();

        // Act
        await _sut.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        recordedDuration.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should record error metrics on failure")]
    public async Task Handle_ShouldRecordErrorMetrics()
    {
        // Arrange
        var request = new TestRequest();
        var exception = new Exception("Test error");
        _next.Invoke().Returns(Task.FromException<string>(exception));

        var recordedError = false;
        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == "ReSys.Shop" && instrument.Name == "usecase.errors")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "usecase.errors")
            {
                recordedError = true;
                measurement.Should().Be(1);
                var tagDict = new Dictionary<string, object?>();
                foreach (var tag in tags) tagDict[tag.Key] = tag.Value;
                tagDict["usecase"].Should().Be("TestRequest");
            }
        });
        meterListener.Start();

        // Act
        var act = () => _sut.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        recordedError.Should().BeTrue();
    }

    public record TestRequest : IRequest<string>;
}

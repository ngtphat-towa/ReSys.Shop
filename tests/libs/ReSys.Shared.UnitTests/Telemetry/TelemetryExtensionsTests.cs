using System.Diagnostics;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using ReSys.Shared.Telemetry;

namespace ReSys.Shared.UnitTests.Telemetry;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Type", "Telemetry")]
public class TelemetryExtensionsTests
{
    [Fact(DisplayName = "RegisterModule: Should execute action and record activity")]
    public void RegisterModule_Should_ExecuteAction_And_RecordActivity()
    {
        // Arrange
        var services = new ServiceCollection();
        var actionExecuted = false;
        Activity? recordedActivity = null;

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == TelemetryConstants.ServiceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => recordedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        services.RegisterModule("TestLayer", "TestModule", s =>
        {
            actionExecuted = true;
            s.Should().BeSameAs(services);
        });

        // Assert
        actionExecuted.Should().BeTrue();
        recordedActivity.Should().NotBeNull();
        recordedActivity!.OperationName.Should().Be("RegisterModule TestLayer.TestModule");
        recordedActivity.TagObjects.Should().Contain(new KeyValuePair<string, object?>("layer", "TestLayer"));
        recordedActivity.TagObjects.Should().Contain(new KeyValuePair<string, object?>("module", "TestModule"));
        recordedActivity.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact(DisplayName = "RegisterModule: Should rethrow exception and set error status on activity")]
    public void RegisterModule_Should_RethrowException_And_SetErrorStatus()
    {
        // Arrange
        var services = new ServiceCollection();
        var exception = new Exception("Module load failed");
        Activity? recordedActivity = null;

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == TelemetryConstants.ServiceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => recordedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        var act = () => services.RegisterModule("TestLayer", "TestModule", s => throw exception);

        // Assert
        act.Should().Throw<Exception>().WithMessage("Module load failed");

        recordedActivity.Should().NotBeNull();
        recordedActivity!.Status.Should().Be(ActivityStatusCode.Error);
        recordedActivity.StatusDescription.Should().Be("Module load failed");
    }
}

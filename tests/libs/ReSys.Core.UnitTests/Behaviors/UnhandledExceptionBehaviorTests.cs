using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ReSys.Core.Common.Behaviors;
using ReSys.Core.UnitTests.TestInfrastructure;
using Xunit;

namespace ReSys.Core.UnitTests.Behaviors;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Type", "Behavior")]
public class UnhandledExceptionBehaviorTests : IClassFixture<TestDatabaseFixture>
{
    private readonly UnhandledExceptionBehavior<TestCommand, ErrorOr<string>> _sut;
    private readonly ILogger<TestCommand> _logger;
    private readonly RequestHandlerDelegate<ErrorOr<string>> _next;

    public UnhandledExceptionBehaviorTests(TestDatabaseFixture fixture)
    {
        _logger = fixture.CreateLogger<TestCommand>();
        _sut = new UnhandledExceptionBehavior<TestCommand, ErrorOr<string>>(_logger);
        _next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();
    }

    [Fact(DisplayName = "Handle: Should call next and return success when no exception occurs")]
    public async Task Handle_Should_CallNext_WhenNoExceptionOccurs()
    {
        // Arrange
        var request = new TestCommand();
        _next.Invoke().Returns(Task.FromResult<ErrorOr<string>>("Success"));

        // Act
        var result = await _sut.Handle(request, _next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("Success");
        await _next.Received(1).Invoke();
    }

    [Fact(DisplayName = "Handle: Should return Unexpected error when exception is thrown")]
    public async Task Handle_Should_ReturnUnexpectedError_WhenExceptionIsThrown()
    {
        // Arrange
        var request = new TestCommand();
        var exception = new Exception("Something went wrong");
        _next.Invoke().Throws(exception);

        // Act
        var result = await _sut.Handle(request, _next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.FirstError.Code.Should().Be("General.UnhandledException");
        
        // Verify logging
        _logger.ReceivedWithAnyArgs(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    public record TestCommand : IRequest<ErrorOr<string>>;
}

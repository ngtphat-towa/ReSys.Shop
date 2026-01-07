using MediatR;


using Microsoft.Extensions.Logging;


using NSubstitute;


using ReSys.Core.Common.Behaviors;

namespace ReSys.Core.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestRequest, string>> _logger;
    private readonly LoggingBehavior<TestRequest, string> _sut;
    private readonly RequestHandlerDelegate<string> _next;

    public LoggingBehaviorTests()
    {
        _logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        _sut = new LoggingBehavior<TestRequest, string>(_logger);
        _next = Substitute.For<RequestHandlerDelegate<string>>();
    }

    [Fact(DisplayName = "Handle: Should log start and end messages and call next")]
    public async Task Handle_ShouldLogAndCallNext()
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
            Arg.Is<object>(o => o.ToString()!.Contains("Processing Request")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Processed Request")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    public record TestRequest : IRequest<string>;
}

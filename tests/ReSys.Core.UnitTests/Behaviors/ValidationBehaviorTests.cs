using ErrorOr;
using FluentValidation;
using MediatR;
using NSubstitute;
using ReSys.Core.Behaviors;

namespace ReSys.Core.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly ValidationBehavior<TestCommand, ErrorOr<string>> _sut;
    private readonly IValidator<TestCommand> _validator;
    private readonly RequestHandlerDelegate<ErrorOr<string>> _next;

    public ValidationBehaviorTests()
    {
        _validator = Substitute.For<IValidator<TestCommand>>();
        _sut = new ValidationBehavior<TestCommand, ErrorOr<string>>([_validator]);
        _next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();
    }

    [Fact(DisplayName = "Handle: Should call next when no validators are present")]
    public async Task Handle_NoValidators_CallsNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestCommand, ErrorOr<string>>([]);
        var request = new TestCommand();
        _next.Invoke().Returns("Success");

        // Act
        var result = await behavior.Handle(request, _next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("Success");
        await _next.Received(1).Invoke();
    }

    [Fact(DisplayName = "Handle: Should call next when validation succeeds")]
    public async Task Handle_ValidationBest_CallsNext()
    {
        // Arrange
        var request = new TestCommand();
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());
        _next.Invoke().Returns("Success");

        // Act
        var result = await _sut.Handle(request, _next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _next.Received(1).Invoke();
    }

    [Fact(DisplayName = "Handle: Should return Validation errors when validation fails")]
    public async Task Handle_ValidationFails_ReturnsErrors()
    {
        // Arrange
        var request = new TestCommand();
        var failure = new FluentValidation.Results.ValidationFailure("PropName", "Error Message");
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult([failure]));

        // Act
        var result = await _sut.Handle(request, _next, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("prop_name");
        result.FirstError.Description.Should().Be("Error Message");
        await _next.DidNotReceive().Invoke();
    }

    public record TestCommand : IRequest<ErrorOr<string>>;
}

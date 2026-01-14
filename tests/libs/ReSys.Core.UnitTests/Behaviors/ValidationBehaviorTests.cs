using ErrorOr;

using FluentValidation;

using MediatR;

using NSubstitute;

using ReSys.Core.Common.Behaviors;

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
        _next.Invoke().Returns(Task.FromResult<ErrorOr<string>>("Success"));

        // Act
        var result = await behavior.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("Success");
        await _next.Received(1).Invoke();
    }

    [Fact(DisplayName = "Handle: Should call next when validation succeeds")]
    public async Task Handle_ValidationSucceeds_CallsNext()
    {
        // Arrange
        var request = new TestCommand { Name = "Valid" };
        _validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
        _next.Invoke().Returns(Task.FromResult<ErrorOr<string>>("Success"));

        // Act
        var result = await _sut.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        await _next.Received(1).Invoke();
    }

    [Fact(DisplayName = "Handle: Should return Validation errors when validation fails")]
    public async Task Handle_ValidationFails_ReturnsErrors()
    {
        // Arrange
        var request = new TestCommand { Name = "" };
        var failure = new FluentValidation.Results.ValidationFailure("PropName", "Error Message");
        _validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult([failure])));

        // Act
        var result = await _sut.Handle(request, _next, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("prop_name"); // Matches ToSnakeCase()
        await _next.DidNotReceive().Invoke();
    }

    public record TestCommand : IRequest<ErrorOr<string>>
    {
        public string Name { get; init; } = string.Empty;
    }
}


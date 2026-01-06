using ReSys.Core.Domain.Identity;
using ReSys.Api.Features.Account.Contracts;
using ReSys.Api.Features.Account.Validators;

namespace ReSys.Api.UnitTests.Features.Account.Validators;

public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _sut = new();

    [Fact]
    public async Task Validate_ShouldReturnTrue_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateUserRequest("test@test.com", "password123", "John", "Doe", UserType.Customer);

        // Act
        var result = await _sut.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldReturnFalse_WhenEmailIsInvalid()
    {
        // Arrange
        var request = new CreateUserRequest("invalid-email", "password123", "John", "Doe", UserType.Customer);

        // Act
        var result = await _sut.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateUserRequest.Email));
    }

    [Fact]
    public async Task Validate_ShouldReturnFalse_WhenPasswordIsShort()
    {
        // Arrange
        var request = new CreateUserRequest("test@test.com", "123", "John", "Doe", UserType.Customer);

        // Act
        var result = await _sut.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateUserRequest.Password));
    }
}

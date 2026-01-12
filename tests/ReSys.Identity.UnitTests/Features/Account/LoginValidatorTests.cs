using FluentAssertions;
using ReSys.Identity.Features.Account;
using Xunit;

namespace ReSys.Identity.UnitTests.Features.Account;

public class LoginValidatorTests
{
    private readonly Login.Validator _validator = new();

    [Fact]
    public void Validator_WithValidData_ShouldPass()
    {
        // Arrange
        var request = new Login.Request("test@test.com", "Password123!", false);
        var command = new Login.Command(request);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password123!", "Data.Email")]
    [InlineData("invalid-email", "Password123!", "Data.Email")]
    [InlineData("test@test.com", "", "Data.Password")]
    public void Validator_WithInvalidData_ShouldFail(string email, string password, string errorField)
    {
        // Arrange
        var request = new Login.Request(email, password, false);
        var command = new Login.Command(request);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == errorField);
    }
}

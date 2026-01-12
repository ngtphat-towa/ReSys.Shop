using FluentAssertions;
using ReSys.Identity.Features.Account;
using Xunit;

namespace ReSys.Identity.UnitTests.Features.Account;

public class RegisterValidatorTests
{
    private readonly Register.Validator _validator = new();

    [Fact]
    public void Validator_WithValidData_ShouldPass()
    {
        // Arrange
        var request = new Register.Request("test@test.com", "Pass123$", "Pass123$");
        var command = new Register.Command(request);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Pass123$", "Pass123$", "Data.Email")]
    [InlineData("invalid-email", "Pass123$", "Pass123$", "Data.Email")]
    [InlineData("test@test.com", "", "Pass123$", "Data.Password")]
    [InlineData("test@test.com", "short", "short", "Data.Password")]
    [InlineData("test@test.com", "Pass123$", "Mismatch", "Data.ConfirmPassword")]
    public void Validator_WithInvalidData_ShouldFail(string email, string password, string confirmPassword, string errorField)
    {
        // Arrange
        var request = new Register.Request(email, password, confirmPassword);
        var command = new Register.Command(request);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == errorField);
    }
}

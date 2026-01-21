using FluentAssertions;
using ReSys.Core.Features.Shared.Identity.Internal.Register;
using ReSys.Core.Domain.Identity.Users;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Auth;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Auth")]
public class RegisterValidationTests
{
    private readonly Register.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when email is empty")]
    public void Validator_ShouldFail_WhenEmailIsEmpty()
    {
        var request = new Register.Request { Email = "", Password = "Password123!", ConfirmPassword = "Password123!" };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == UserErrors.EmailRequired.Code);
    }

    [Fact(DisplayName = "Validator: Should fail when passwords do not match")]
    public void Validator_ShouldFail_WhenPasswordsDoNotMatch()
    {
        var request = new Register.Request { Email = "test@example.com", Password = "Password123!", ConfirmPassword = "DifferentPassword" };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Request.ConfirmPassword));
    }

    [Fact(DisplayName = "Validator: Should fail when password is empty")]
    public void Validator_ShouldFail_WhenPasswordIsEmpty()
    {
        var request = new Register.Request { Email = "test@example.com", Password = "", ConfirmPassword = "" };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Request.Password));
    }
}

using ReSys.Core.Features.Shared.Identity.Internal.Login;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Auth;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Auth")]
public class LoginValidationTests
{
    private readonly Login.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when credential is empty")]
    public void Validator_ShouldFail_WhenCredentialIsEmpty()
    {
        var request = new Login.Request("", "Password123!");
        var result = _validator.Validate(new Login.Command(request));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Credential");
    }

    [Fact(DisplayName = "Validator: Should fail when password is empty")]
    public void Validator_ShouldFail_WhenPasswordIsEmpty()
    {
        var request = new Login.Request("test@example.com", "");
        var result = _validator.Validate(new Login.Command(request));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Password");
    }
}

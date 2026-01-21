using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Password;
using ReSys.Core.Features.Shared.Identity.Common;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Password;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Password")]
public class ResetPasswordTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    [Fact(DisplayName = "Handle: Should successfully reset password with valid token")]
    public async Task Handle_ValidToken_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("reset-pass@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user, "OldPassword123!");

        var rawToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = AccountEncodingExtensions.EncodeToken(rawToken);

        var handler = new ResetPassword.Handler(userManager);
        var command = new ResetPassword.Command(new ResetPassword.Request(user.Email!, encodedToken, "NewPassword123!"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        (await userManager.CheckPasswordAsync(user, "NewPassword123!")).Should().BeTrue();
    }
}

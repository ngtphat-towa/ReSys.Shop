using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Confirmations;
using ReSys.Core.Features.Shared.Identity.Common;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Confirmations;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Confirmations")]
public class ConfirmEmailTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    [Fact(DisplayName = "Handle: Should successfully confirm email with valid code")]
    public async Task Handle_ValidCode_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("confirm@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user);

        var rawCode = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = AccountEncodingExtensions.EncodeToken(rawCode);

        var handler = new ConfirmEmail.Handler(userManager);
        var command = new ConfirmEmail.Command(new ConfirmEmail.Request(user.Id, encodedCode));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        (await userManager.IsEmailConfirmedAsync(user)).Should().BeTrue();
    }
}

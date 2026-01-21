using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Confirmations;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Confirmations;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Confirmations")]
public class ConfirmPhoneTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    [Fact(DisplayName = "Handle: Should successfully confirm phone with valid code")]
    public async Task Handle_ValidCode_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("phone@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        user.PhoneNumber = "+420777123456";
        await userManager.CreateAsync(user);

        var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);

        var handler = new ConfirmPhone.Handler(userManager);
        var command = new ConfirmPhone.Command(new ConfirmPhone.Request(user.Id, user.PhoneNumber, code));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        (await userManager.IsPhoneNumberConfirmedAsync(user)).Should().BeTrue();
    }
}

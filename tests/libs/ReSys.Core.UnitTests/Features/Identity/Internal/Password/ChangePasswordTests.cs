using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Password;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Password;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Password")]
public class ChangePasswordTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    [Fact(DisplayName = "Handle: Should successfully change password for authenticated user")]
    public async Task Handle_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("change-pass@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user, "OldPassword123!");

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(user.Id);

        var handler = new ChangePassword.Handler(userManager, _userContext);
        var command = new ChangePassword.Command(new ChangePassword.Request("OldPassword123!", "NewPassword123!"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        (await userManager.CheckPasswordAsync(user, "NewPassword123!")).Should().BeTrue();
    }
}

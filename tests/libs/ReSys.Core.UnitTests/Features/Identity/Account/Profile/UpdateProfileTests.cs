using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Account.Profile;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;
using Mapster;

namespace ReSys.Core.UnitTests.Features.Identity.Account.Profile;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Profile")]
public class UpdateProfileTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    static UpdateProfileTests()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<User, UserProfileResponse>();
    }

    [Fact(DisplayName = "Handle: Should successfully update profile details")]
    public async Task Handle_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("jane@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user);

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(user.Id);

        var handler = new UpdateProfile.Handler(_userContext, userManager, context);
        var request = new UpdateProfile.Request("Jane", "Doe", null, null, null, null, null);

        // Act
        var result = await handler.Handle(new UpdateProfile.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        user.FirstName.Should().Be("Jane");
    }
}
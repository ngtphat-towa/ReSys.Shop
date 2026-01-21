using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Sessions;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;
using Mapster;
using ReSys.Core.Features.Shared.Identity.Internal.Common;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Sessions;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Sessions")]
public class GetSessionTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IAuthorizeClaimDataProvider _authDataProvider = Substitute.For<IAuthorizeClaimDataProvider>();

    static GetSessionTests()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<User, GetSession.Response>();
    }

    [Fact(DisplayName = "Handle: Should return user profile and permissions when authenticated")]
    public async Task Handle_Authenticated_ShouldReturnResponse()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("session@example.com", "sessionuser", "Session", "User").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user);

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(user.Id);

        var authData = new AuthorizeClaimData(
            user.Id,
            user.UserName!,
            user.Email!,
            new List<string> { "storefront.orders.view" },
            new List<string> { "Customer" },
            new List<string>());

        _authDataProvider.GetUserAuthorizationAsync(user.Id).Returns(authData);

        var handler = new GetSession.Handler(_userContext, userManager, _authDataProvider);

        // Act
        var result = await handler.Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Email.Should().Be(user.Email);
        result.Value.Roles.Should().Contain("Customer");
        result.Value.Permissions.Should().Contain("storefront.orders.view");
    }

    [Fact(DisplayName = "Handle: Should return Unauthorized when user is not authenticated")]
    public async Task Handle_NotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        _userContext.IsAuthenticated.Returns(false);

        var handler = new GetSession.Handler(_userContext, userManager, _authDataProvider);

        // Act
        var result = await handler.Handle(new GetSession.Query(), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("User.Unauthorized");
    }
}

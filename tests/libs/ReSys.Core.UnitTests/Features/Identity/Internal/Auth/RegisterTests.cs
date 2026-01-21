using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Mapster;

using MediatR;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Features.Shared.Identity.Internal.Register;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Auth;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Auth")]
public class RegisterTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    static RegisterTests()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<User, UserProfileResponse>();
    }

    [Fact(DisplayName = "Handle: Should successfully register user and assign role")]

    public async Task Handle_ValidRequest_ShouldSucceed()

    {

        // Arrange

        var (context, sp) = CreateTestContext();

        var userManager = sp.GetRequiredService<UserManager<User>>();

        var roleManager = sp.GetRequiredService<RoleManager<Role>>();

        var mediator = sp.GetRequiredService<IMediator>();



        await roleManager.CreateAsync(Role.Create("Storefront.Customer").Value);



        var request = new Register.Request
        {

            Email = "new@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"

        };



        var handler = new Register.Handler(userManager, roleManager);



        // Act

        var result = await handler.Handle(new Register.Command(request), TestContext.Current.CancellationToken);



        // Assert

        result.IsError.Should().BeFalse(result.IsError ? $"{result.FirstError.Code}: {result.FirstError.Description}" : "");

        var user = await userManager.FindByEmailAsync("new@example.com");

        user.Should().NotBeNull();

        (await userManager.IsInRoleAsync(user!, "Storefront.Customer")).Should().BeTrue();



        // Verify domain event was published via interceptor

        await mediator.Received(1).Publish(Arg.Any<UserEvents.UserCreated>(), Arg.Any<CancellationToken>());

    }



    [Fact(DisplayName = "Handle: Should fail when email already exists")]

    public async Task Handle_EmailExists_ShouldReturnError()

    {

        // Arrange

        var (context, sp) = CreateTestContext();

        var userManager = sp.GetRequiredService<UserManager<User>>();

        var roleManager = sp.GetRequiredService<RoleManager<Role>>();



        var existingUser = User.Create("existing@example.com").Value;

        await userManager.CreateAsync(existingUser);



        var request = new Register.Request
        {

            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"

        };



        var handler = new Register.Handler(userManager, roleManager);



        // Act

        var result = await handler.Handle(new Register.Command(request), TestContext.Current.CancellationToken);



        // Assert

        result.IsError.Should().BeTrue();

        result.FirstError.Code.Should().Be("User.EmailAlreadyExists");

    }



    [Fact(DisplayName = "Handle: Should fail when default role is missing")]

    public async Task Handle_RoleMissing_ShouldReturnError()

    {

        // Arrange

        var (context, sp) = CreateTestContext();

        var userManager = sp.GetRequiredService<UserManager<User>>();

        var roleManager = sp.GetRequiredService<RoleManager<Role>>();



        // We don't create the role here



        var request = new Register.Request
        {

            Email = "norole@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"

        };



        var handler = new Register.Handler(userManager, roleManager);

        // Act

        var result = await handler.Handle(new Register.Command(request), TestContext.Current.CancellationToken);

        // Assert

        result.IsError.Should().BeTrue();

        result.FirstError.Code.Should().Be("Role.NotFound");

    }
}

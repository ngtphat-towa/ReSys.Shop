using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using MediatR;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Password;
using ReSys.Core.Common.Data;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Password;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Password")]
public class ForgotPasswordTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    [Fact(DisplayName = "Handle: Should raise PasswordResetRequested when user exists")]
    public async Task Handle_UserExists_ShouldRaiseEvent()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("test@example.com").Value;
        await userManager.CreateAsync(user);
        user.ClearDomainEvents();

        var handler = new ForgotPassword.Handler(userManager, context);
        var command = new ForgotPassword.Command(new ForgotPassword.Request("test@example.com"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Message.Should().NotBeNullOrEmpty();
        
        // The interceptor publishes via IMediator and clears events
        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.Received(1).Publish(Arg.Any<UserEvents.PasswordResetRequested>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Handle: Should return success message even if user not found (security)")]
    public async Task Handle_UserNotFound_ShouldReturnSuccessMessage()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();

        var handler = new ForgotPassword.Handler(userManager, context);
        var command = new ForgotPassword.Command(new ForgotPassword.Request("nonexistent@example.com"));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Message.Should().Be("If your email is registered, you will receive a reset link shortly.");
    }
}

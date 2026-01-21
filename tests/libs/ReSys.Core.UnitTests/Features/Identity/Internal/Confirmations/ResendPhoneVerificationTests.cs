using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using MediatR;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Confirmations;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Identity.Internal.Confirmations;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Confirmations")]
public class ResendPhoneVerificationTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    [Fact(DisplayName = "Handle: Should raise PhoneConfirmationRequested domain event")]
    public async Task Handle_UserExists_ShouldRaiseEvent()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("phone-resend@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        user.PhoneNumber = "+420111222333";
        await userManager.CreateAsync(user);

        var handler = new ResendPhoneVerification.Handler(userManager, context);
        var command = new ResendPhoneVerification.Command(new ResendPhoneVerification.Request(user.PhoneNumber));

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.Received(1).Publish(Arg.Any<UserEvents.PhoneConfirmationRequested>(), Arg.Any<CancellationToken>());
    }
}

using ReSys.Core.Domain.Identity.Users;
using FluentAssertions;

namespace ReSys.Core.UnitTests.Domain.Identity.Users;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Domain", "UserEvents")]
public class IdentityDomainEventTests
{
    private const string ValidEmail = "test@example.com";

    [Fact(DisplayName = "RequestPasswordReset: Should raise PasswordResetRequested event")]
    public void RequestPasswordReset_Should_RaiseEvent()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        user.ClearDomainEvents();

        // Act
        user.RequestPasswordReset();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserEvents.PasswordResetRequested);
        var @event = (UserEvents.PasswordResetRequested)user.DomainEvents.First();
        @event.User.Should().Be(user);
    }

    [Fact(DisplayName = "RequestEmailChange: Should raise EmailChangeRequested event with new email")]
    public void RequestEmailChange_Should_RaiseEvent()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        user.ClearDomainEvents();
        var newEmail = "new@example.com";

        // Act
        user.RequestEmailChange(newEmail);

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserEvents.EmailChangeRequested);
        var @event = (UserEvents.EmailChangeRequested)user.DomainEvents.First();
        @event.User.Should().Be(user);
        @event.NewEmail.Should().Be(newEmail);
    }

    [Fact(DisplayName = "RequestPhoneChange: Should raise PhoneChangeRequested event with new phone")]
    public void RequestPhoneChange_Should_RaiseEvent()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        user.ClearDomainEvents();
        var newPhone = "+420777123456";

        // Act
        user.RequestPhoneChange(newPhone);

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserEvents.PhoneChangeRequested);
        var @event = (UserEvents.PhoneChangeRequested)user.DomainEvents.First();
        @event.User.Should().Be(user);
        @event.NewPhone.Should().Be(newPhone);
    }

    [Fact(DisplayName = "RequestEmailConfirmation: Should raise EmailConfirmationRequested event")]
    public void RequestEmailConfirmation_Should_RaiseEvent()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        user.ClearDomainEvents();

        // Act
        user.RequestEmailConfirmation();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserEvents.EmailConfirmationRequested);
        var @event = (UserEvents.EmailConfirmationRequested)user.DomainEvents.First();
        @event.User.Should().Be(user);
    }

    [Fact(DisplayName = "RequestPhoneConfirmation: Should raise PhoneConfirmationRequested event")]
    public void RequestPhoneConfirmation_Should_RaiseEvent()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        user.ClearDomainEvents();

        // Act
        user.RequestPhoneConfirmation();

        // Assert
        user.DomainEvents.Should().ContainSingle(e => e is UserEvents.PhoneConfirmationRequested);
        var @event = (UserEvents.PhoneConfirmationRequested)user.DomainEvents.First();
        @event.User.Should().Be(user);
    }

    [Fact(DisplayName = "Domain Methods: Should update UpdatedAt timestamp")]
    public void DomainMethods_Should_UpdateTimestamp()
    {
        // Arrange
        var user = User.Create(ValidEmail).Value;
        var initialUpdatedAt = user.UpdatedAt;
        Thread.Sleep(10); // Ensure timestamp difference

        // Act
        user.RequestPasswordReset();

        // Assert
        user.UpdatedAt.Should().NotBe(initialUpdatedAt);
    }
}

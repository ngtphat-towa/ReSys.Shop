using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Core.UnitTests.Domain.Settings.PaymentMethods;

[Trait("Category", "Unit")]
[Trait("Module", "Settings")]
[Trait("Domain", "PaymentMethod")]
public class PaymentMethodTests
{
    [Fact(DisplayName = "Create: Should successfully initialize payment method")]
    public void Create_Should_InitializePaymentMethod()
    {
        // Act
        var result = PaymentMethod.Create("Stripe", "Stripe Checkout", PaymentMethod.PaymentType.Stripe, "Credit card processing", 1, true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Stripe");
        result.Value.Presentation.Should().Be("Stripe Checkout");
        result.Value.Type.Should().Be(PaymentMethod.PaymentType.Stripe);
        result.Value.AutoCapture.Should().BeTrue();
        result.Value.Status.Should().Be(PaymentMethod.PaymentStatus.Active);
        result.Value.DomainEvents.Should().ContainSingle(e => e is PaymentMethodEvents.PaymentMethodCreated);
    }

    [Fact(DisplayName = "UpdateDetails: Should change properties")]
    public void UpdateDetails_Should_ChangeProperties()
    {
        // Arrange
        var method = PaymentMethod.Create("Bank", "Bank Transfer", PaymentMethod.PaymentType.BankTransfer).Value;
        method.ClearDomainEvents();

        // Act
        var result = method.UpdateDetails("New Bank", "Wire Transfer", "New desc", 5, true);

        // Assert
        result.IsError.Should().BeFalse();
        method.Name.Should().Be("New Bank");
        method.Presentation.Should().Be("Wire Transfer");
        method.Position.Should().Be(5);
        method.AutoCapture.Should().BeTrue();
        method.DomainEvents.Should().ContainSingle(e => e is PaymentMethodEvents.PaymentMethodUpdated);
    }

    [Fact(DisplayName = "Status: Should transition between states")]
    public void Status_Should_TransitionStates()
    {
        // Arrange
        var method = PaymentMethod.Create("Cash", "COD", PaymentMethod.PaymentType.CashOnDelivery).Value;

        // Act & Assert: Deactivate
        method.Deactivate();
        method.Status.Should().Be(PaymentMethod.PaymentStatus.Inactive);

        // Act & Assert: Activate
        method.Activate();
        method.Status.Should().Be(PaymentMethod.PaymentStatus.Active);

        // Act & Assert: Archive
        method.Archive();
        method.Status.Should().Be(PaymentMethod.PaymentStatus.Archived);
    }

    [Fact(DisplayName = "Delete: Should set soft delete state")]
    public void Delete_Should_SetSoftDeleted()
    {
        // Arrange
        var method = PaymentMethod.Create("Delete Me", "DM", PaymentMethod.PaymentType.StoreCredit).Value;

        // Act
        method.Delete();

        // Assert
        method.IsDeleted.Should().BeTrue();
        method.DeletedAt.Should().NotBeNull();
        method.DomainEvents.Should().Contain(e => e is PaymentMethodEvents.PaymentMethodDeleted);
    }
}

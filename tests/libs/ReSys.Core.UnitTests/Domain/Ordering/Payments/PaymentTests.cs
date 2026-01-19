using ReSys.Core.Domain.Ordering.Payments;

namespace ReSys.Core.UnitTests.Domain.Ordering.Payments;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Domain", "Payment")]
public class PaymentTests
{
    private readonly Guid _orderId = Guid.NewGuid();

    [Fact(DisplayName = "Create: Should successfully initialize payment")]
    public void Create_Should_InitializePayment()
    {
        // Act
        var result = Payment.Create(_orderId, 1000, "USD", "CreditCard");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.OrderId.Should().Be(_orderId);
        result.Value.AmountCents.Should().Be(1000);
        result.Value.State.Should().Be(Payment.PaymentState.Pending);
        result.Value.DomainEvents.Should().ContainSingle(e => e is PaymentEvents.PaymentCreated);
    }

    [Fact(DisplayName = "StateMachine: Should flow from Pending to Authorized to Completed")]
    public void StateMachine_Should_FlowToCompletion()
    {
        // 1. Arrange
        var payment = Payment.Create(_orderId, 1000, "USD", "CreditCard").Value;

        // 2. Act & Assert: Authorize
        payment.MarkAsAuthorized("TXN-AUTH", "AUTH-123").IsError.Should().BeFalse();
        payment.State.Should().Be(Payment.PaymentState.Authorized);
        payment.AuthorizedAt.Should().NotBeNull();

        // 3. Act & Assert: Capture
        payment.MarkAsCaptured().IsError.Should().BeFalse();
        payment.State.Should().Be(Payment.PaymentState.Completed);
        payment.CapturedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Refund: Should support partial and full refunds")]
    public void Refund_Should_SupportPartialAndFull()
    {
        // Arrange
        var payment = Payment.Create(_orderId, 1000, "USD", "CreditCard").Value;
        payment.MarkAsAuthorized("TXN-1");
        payment.MarkAsCaptured();

        // Act 1: Partial Refund
        var result1 = payment.Refund(400, "Partial return");
        
        // Assert 1
        result1.IsError.Should().BeFalse();
        payment.RefundedAmountCents.Should().Be(400);
        payment.State.Should().Be(Payment.PaymentState.Completed); // Still completed as long as partial

        // Act 2: Final Refund
        var result2 = payment.Refund(600, "Full return");

        // Assert 2
        result2.IsError.Should().BeFalse();
        payment.RefundedAmountCents.Should().Be(1000);
        payment.State.Should().Be(Payment.PaymentState.Refunded);
    }

    [Fact(DisplayName = "Refund: Should fail if amount exceeds balance")]
    public void Refund_ShouldFail_IfExceedsBalance()
    {
        // Arrange
        var payment = Payment.Create(_orderId, 1000, "USD", "CreditCard").Value;
        payment.MarkAsAuthorized("TXN-1");
        payment.MarkAsCaptured();

        // Act
        var result = payment.Refund(1100, "Over refund");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Payment.RefundExceedsBalance");
    }

    [Fact(DisplayName = "Void: Should cancel authorized payment")]
    public void Void_Should_WorkIfAuthorized()
    {
        // Arrange
        var payment = Payment.Create(_orderId, 1000, "USD", "CreditCard").Value;
        payment.MarkAsAuthorized("TXN-1");

        // Act
        var result = payment.Void();

        // Assert
        result.IsError.Should().BeFalse();
        payment.State.Should().Be(Payment.PaymentState.Void);
        payment.VoidedAt.Should().NotBeNull();
    }
}

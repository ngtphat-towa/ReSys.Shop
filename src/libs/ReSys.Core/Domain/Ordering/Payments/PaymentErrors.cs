using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Payment.NotFound",
        description: $"Payment with ID '{id}' was not found.");

    public static Error CurrencyMismatch => Error.Validation(
        code: "Payment.CurrencyMismatch",
        description: "Payment currency does not match order currency.");

    public static Error CurrencyRequired => Error.Validation(
        code: "Payment.CurrencyRequired",
        description: "Payment currency is required.");

    public static Error PaymentMethodTypeRequired => Error.Validation(
        code: "Payment.PaymentMethodTypeRequired",
        description: "Payment method type is required.");

    public static Error ReferenceTransactionIdRequired => Error.Validation(
        code: "Payment.ReferenceTransactionIdRequired",
        description: "Reference transaction ID is required.");

    public static Error AlreadyCaptured => Error.Validation(
        code: "Payment.AlreadyCaptured", 
        description: "Payment already captured.");

    public static Error CannotVoidCaptured => Error.Validation(
        code: "Payment.CannotVoidCaptured", 
        description: "Cannot void captured or completed payment.");

    public static Error InvalidAmountCents => Error.Validation(
        code: "Payment.InvalidAmountCents", 
        description: $"Amount cents must be at least {PaymentConstraints.AmountCentsMinValue}.");

    public static Error InvalidStateTransition(Payment.PaymentState from, Payment.PaymentState to) => Error.Validation(
        code: "Payment.InvalidStateTransition", 
        description: $"Cannot transition from {from} to {to}.");

    public static Error AuthorizationRequired => Error.Validation(
        code: "Payment.AuthorizationRequired", 
        description: "Payment must be authorized before capture.");

    public static Error RefundExceedsBalance(decimal requested, decimal balance) => Error.Validation(
        code: "Payment.RefundExceedsBalance",
        description: $"Refund amount ({requested:C}) exceeds remaining payment balance ({balance:C}).");
}

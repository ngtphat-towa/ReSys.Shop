using ErrorOr;

namespace ReSys.Core.Domain.Orders.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Payment.NotFound",
        description: $"Payment with ID '{id}' was not found.");

    public static Error CurrencyMismatch => Error.Validation(
        code: "Payment.CurrencyMismatch",
        description: "Payment currency does not match order currency.");
}

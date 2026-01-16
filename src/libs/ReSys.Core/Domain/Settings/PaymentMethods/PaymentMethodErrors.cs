using ErrorOr;

namespace ReSys.Core.Domain.Settings.PaymentMethods;

public static class PaymentMethodErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "PaymentMethod.NotFound",
        description: $"Payment method with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "PaymentMethod.NameRequired",
        description: "Name is required.");

    public static Error InUse => Error.Conflict(
        code: "PaymentMethod.InUse",
        description: "Cannot delete payment method that is in use.");
}
